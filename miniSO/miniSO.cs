// José Waldo Saraiva Câmara Neto
// RA: 22308422

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Diagnostics;

// Classe usuário
public class Usuario
{
    public string Nome { get; }
    public string SenhaHash { get; }
    private string Salt { get; }

    public Usuario(string nome, string senha)
    {
        Nome = nome;
        Salt = GerarSalt();
        SenhaHash = GerarHashSenha(senha, Salt);
    }

    public Usuario(string nome, string senhaHash, string salt)
    {
        Nome = nome;
        SenhaHash = senhaHash;
        Salt = salt;
    }

    private static string GerarSalt()
    {
        var saltBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }

    private static string GerarHashSenha(string senha, string salt)
    {
        using (var hash = SHA512.Create())
        {
            byte[] hashBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(senha + salt));
            return $"$6${salt}${Convert.ToBase64String(hashBytes)}";
        }
    }

    public bool VerificarSenha(string senha)
    {
        return SenhaHash == GerarHashSenha(senha, Salt);
    }
}

// Classe do gerenciador de usuários
public class GerenciadorUsuarios
{
    private List<Usuario> usuarios = new List<Usuario>();
    private readonly string filePath = ".usuarios";

    public GerenciadorUsuarios()
    {
        CarregarUsuarios();
    }

    public void CriarUsuario(string nome, string senha)
    {
        if (usuarios.Exists(u => u.Nome == nome))
            throw new InvalidOperationException("Usuario já existe.");

        usuarios.Add(new Usuario(nome, senha));
        SalvarUsuarios();
    }

    public Usuario Autenticar(string nome, string senha)
    {
        var usuario = usuarios.Find(u => u.Nome == nome);
        if (usuario != null && usuario.VerificarSenha(senha))
            return usuario;
        else
            throw new UnauthorizedAccessException("Usuário ou senha incorretos.");
    }

    public bool ExistemUsuariosCadastrados() => usuarios.Count > 0;

    private void SalvarUsuarios()
    {
        using (var stream = new FileStream(filePath, FileMode.Create))
        using (var writer = new StreamWriter(stream))
        {
            foreach (var usuario in usuarios)
            {
                writer.WriteLine($"{usuario.Nome}:{usuario.SenhaHash}");
            }
        }
    }

    private void CarregarUsuarios()
    {
        if (!File.Exists(filePath))
            return;

        using (var stream = new FileStream(filePath, FileMode.Open))
        using (var reader = new StreamReader(stream))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(':');
                if (parts.Length == 2)
                {
                    var hashParts = parts[1].Split('$');
                    if (hashParts.Length == 4 && hashParts[1] == "6")
                    {
                        var usuario = new Usuario(parts[0], parts[1], hashParts[2]);
                        usuarios.Add(usuario);
                    }
                }
            }
        }
    }
}

// Classe do gerenciador de memória
public class GerenciadorMemoria
{
    private List<Process> processos = new List<Process>();

    public Process CriarProcesso(string nome, string argumentos)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = $"/C {argumentos}";
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;

        Process processo = new Process();
        processo.StartInfo = startInfo;
        processo.Start();

        processos.Add(processo);

        Console.WriteLine($"Processo {processo.Id} criado para '{nome}'.");

        return processo;
    }

    public void TerminarProcesso(int pid)
    {
        var processo = processos.Find(p => p.Id == pid);
        if (processo != null)
        {
            processo.Kill();
            processos.Remove(processo);
            Console.WriteLine($"Processo {processo.Id} terminado.");
        }
    }
}

// Implementação dos comandos do Shell
public class MiniShell
{
    private GerenciadorUsuarios gerenciadorUsuarios = new GerenciadorUsuarios();
    private GerenciadorMemoria gerenciadorMemoria = new GerenciadorMemoria();
    public Usuario usuarioAtual;

    public void Iniciar()
    {
        if (!gerenciadorUsuarios.ExistemUsuariosCadastrados())
        {
            var processo = gerenciadorMemoria.CriarProcesso("criar usuario", "echo Crie um usuário para o primeiro acesso.");
            Console.WriteLine($"Processo {processo.Id} executando comando 'criar usuario'");

            Console.Write("Nome de usuário: ");
            string nome = Console.ReadLine();
            Console.Write("Senha: ");
            string senha = LerSenha();

            gerenciadorUsuarios.CriarUsuario(nome, senha);
            Console.WriteLine("Usuário criado com sucesso.");

            gerenciadorMemoria.TerminarProcesso(processo.Id);
        }

        AutenticarUsuario();
    }

    private void AutenticarUsuario()
    {
        var processo = gerenciadorMemoria.CriarProcesso("autenticar usuario", "echo Autenticando usuário.");
        Console.WriteLine($"Processo {processo.Id} executando comando 'autenticar usuario'");

        Console.Write("Nome de usuário: ");
        string nome = Console.ReadLine();
        Console.Write("Senha: ");
        string senha = LerSenha();
        try
        {
            usuarioAtual = gerenciadorUsuarios.Autenticar(nome, senha);
            Console.WriteLine($"Bem-vindo, {usuarioAtual.Nome}!");
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine(e.Message);
            AutenticarUsuario();
        }

        gerenciadorMemoria.TerminarProcesso(processo.Id);
    }

    private string LerSenha()
    {
        StringBuilder senha = new StringBuilder();
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(intercept: true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                senha.Append(key.KeyChar);
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && senha.Length > 0)
            {
                senha.Remove(senha.Length - 1, 1);
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);
        Console.WriteLine();
        return senha.ToString();
    }

    public void ExecutarComando(string comando)
    {
        var partes = comando.Split(' ', 2);
        string comandoPrincipal = partes[0];
        string argumento = partes.Length > 1 ? partes[1] : string.Empty;

        if (comandoPrincipal == "help")
        {
            var processo = gerenciadorMemoria.CriarProcesso("help", "echo Mostrando ajuda.");
            Console.WriteLine($"Processo {processo.Id} executando comando 'help'");
            MostrarHelp();
            gerenciadorMemoria.TerminarProcesso(processo.Id);
        }
        else if (comandoPrincipal == "criar" && argumento.StartsWith("usuario"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("criar usuario", "echo Criando usuário.");
            Console.WriteLine($"Processo {processo.Id} executando comando 'criar usuario'");
            CriarUsuario();
            gerenciadorMemoria.TerminarProcesso(processo.Id);
        }
        else if (comandoPrincipal == "trocar" && argumento.StartsWith("usuario"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("trocar usuario", "echo Trocando usuário.");
            Console.WriteLine($"Processo {processo.Id} executando comando 'trocar usuario'");
            TrocarUsuario();
            gerenciadorMemoria.TerminarProcesso(processo.Id);
        }
        else if (comandoPrincipal == "listar")
        {
            string dir = argumento.Length > 0 ? argumento : Directory.GetCurrentDirectory();
            var processo = gerenciadorMemoria.CriarProcesso("listar", $"dir {dir}");
            Console.WriteLine($"Processo {processo.Id} executando comando 'listar'");
            ListarArquivos(dir);
            gerenciadorMemoria.TerminarProcesso(processo.Id);
        }
        else if (comandoPrincipal == "criar" && argumento.StartsWith("arquivo"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("criar arquivo", "echo Criando arquivo.");
            Console.WriteLine($"Processo {processo.Id} executando comando 'criar arquivo'");
            CriarArquivo(argumento);
            gerenciadorMemoria.TerminarProcesso(processo.Id);
        }
        else if (comandoPrincipal == "apagar" && argumento.StartsWith("arquivo"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("apagar arquivo", "echo Apagando arquivo.");
            Console.WriteLine($"Processo {processo.Id} executando comando 'apagar arquivo'");
            ApagarArquivo(argumento);
            gerenciadorMemoria.TerminarProcesso(processo.Id);
        }
        else if (comandoPrincipal == "criar" && argumento.StartsWith("diretorio"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("criar diretorio", "echo Criando diretório.");
            Console.WriteLine($"Processo {processo.Id} executando comando 'criar diretorio'");
            CriarDiretorio(argumento);
            gerenciadorMemoria.TerminarProcesso(processo.Id);
        }
        else if (comandoPrincipal == "apagar" && argumento.StartsWith("diretorio"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("apagar diretorio", "echo Apagando diretório.");
            Console.WriteLine($"Processo {processo.Id} executando comando 'apagar diretorio'");
            ApagarDiretorio(argumento);
            gerenciadorMemoria.TerminarProcesso(processo.Id);
        }
        else
        {
            Console.WriteLine("Comando não reconhecido.");
        }
    }

    private void MostrarHelp()
    {
        Console.WriteLine("Comandos disponíveis:");
        Console.WriteLine("  help                      - Mostra esta mensagem de ajuda.");
        Console.WriteLine("  criar usuario             - Cria um novo usuário.");
        Console.WriteLine("  trocar usuario            - Troca o usuário atual.");
        Console.WriteLine("  listar [<dir>]            - Lista os arquivos no diretório especificado ou no diretório atual se nenhum for especificado.");        Console.WriteLine("  criar arquivo <caminho>   - Cria um arquivo no caminho especificado.");
        Console.WriteLine("  apagar arquivo <caminho>  - Apaga o arquivo no caminho especificado.");
        Console.WriteLine("  criar diretorio <caminho> - Cria um diretório no caminho especificado.");
        Console.WriteLine("  apagar diretorio <caminho> [--force] - Apaga o diretório especificado. Use --force para apagar diretórios não vazios.");
        Console.WriteLine("  sair                      - Encerra o shell.");
    }

    private void CriarUsuario()
    {
        Console.Write("Digite o nome do novo usuário: ");
        string nomeUsuario = Console.ReadLine();
        Console.Write("Digite a senha: ");
        string senha = LerSenha();

        gerenciadorUsuarios.CriarUsuario(nomeUsuario, senha);
        Console.WriteLine("Usuário criado com sucesso.");
    }

    private void TrocarUsuario()
    {
        Console.WriteLine("Trocar de usuário...");
        usuarioAtual = null;
        AutenticarUsuario();
    }

    private void ListarArquivos(string dir)
    {
        try
        {
            var arquivos = Directory.GetFiles(dir);
            var diretorios = Directory.GetDirectories(dir);

            Console.WriteLine($"Conteúdo do diretório '{dir}':");
            foreach (var diretorio in diretorios)
            {
                Console.WriteLine($"<DIR> {Path.GetFileName(diretorio)}");
            }
            foreach (var arquivo in arquivos)
            {
                Console.WriteLine(Path.GetFileName(arquivo));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao listar arquivos: {ex.Message}");
        }
    }

    private void CriarArquivo(string argumento)
    {
        var partes = argumento.Split(' ', 2);
        if (partes.Length < 2)
        {
            Console.WriteLine("Uso: criar arquivo <caminho/arquivo.txt>");
            return;
        }
        string caminhoArquivo = partes[1];
        string conteudo = "placeholder";

        try
        {
            File.WriteAllText(caminhoArquivo, $"{usuarioAtual.Nome}:{conteudo}");
            Console.WriteLine($"Arquivo '{caminhoArquivo}' criado com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar arquivo: {ex.Message}");
        }
    }

    private void ApagarArquivo(string argumento)
    {
        var partes = argumento.Split(' ', 2);
        if (partes.Length < 2)
        {
            Console.WriteLine("Uso: apagar arquivo <caminho/arquivo.txt>");
            return;
        }

        string caminhoArquivo = partes[1];

        try
        {
            string[] linhas = File.ReadAllLines(caminhoArquivo);
            if (linhas.Length > 0 && linhas[0].StartsWith($"{usuarioAtual.Nome}:"))
            {
                File.Delete(caminhoArquivo);
                Console.WriteLine($"Arquivo '{caminhoArquivo}' apagado com sucesso.");
            }
            else
            {
                Console.WriteLine("Você não tem permissão para apagar este arquivo.");
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Arquivo não encontrado.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao apagar arquivo: {ex.Message}");
        }
    }

    private void CriarDiretorio(string argumento)
    {
        var partes = argumento.Split(' ', 2);
        if (partes.Length < 2)
        {
            Console.WriteLine("Uso: criar diretorio <caminho/diretorio>");
            return;
        }

        string caminhoDiretorio = partes[1];

        try
        {
            Directory.CreateDirectory(caminhoDiretorio);
            File.WriteAllText(Path.Combine(caminhoDiretorio, ".owner"), usuarioAtual.Nome);
            Console.WriteLine($"Diretório '{caminhoDiretorio}' criado com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar diretório: {ex.Message}");
        }
    }

    private void ApagarDiretorio(string argumento)
    {
        var partes = argumento.Split(' ', 3);
        bool forcar = partes.Length > 2 && partes[2] == "--force";

        if (partes.Length < 2)
        {
            Console.WriteLine("Uso: apagar diretorio <caminho/diretorio> [--force]");
            return;
        }

        string caminhoDiretorio = partes[1];

        try
        {
            string ownerFile = Path.Combine(caminhoDiretorio, ".owner");
            if (File.Exists(ownerFile) && File.ReadAllText(ownerFile) == usuarioAtual.Nome)
            {
                if (forcar)
                {
                    Directory.Delete(caminhoDiretorio, true);
                    Console.WriteLine($"Diretório '{caminhoDiretorio}' apagado com sucesso (forçado).");
                }
                else
                {
                    Directory.Delete(caminhoDiretorio);
                    Console.WriteLine($"Diretório '{caminhoDiretorio}' apagado com sucesso.");
                }
            }
            else
            {
                Console.WriteLine("Você não tem permissão para apagar este diretório.");
            }
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("Diretório não encontrado.");
        }
        catch (IOException)
        {
            Console.WriteLine("Diretório não está vazio, use --force para apagar.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao apagar diretório: {ex.Message}");
        }
    }
}

// Classe do programa em si
class Programa
{
    static void Main(string[] args)
    {
        MiniShell shell = new MiniShell();

        Console.WriteLine("Bem-vindo ao miniSO");
        shell.Iniciar();

        while (true)
        {
            Console.Write($"{shell.usuarioAtual.Nome}> ");
            string comando = Console.ReadLine();

            if (comando.ToLower() == "sair")
            {
                Console.WriteLine("Encerrando miniSO");
                break;
            }

            try
            {
                shell.ExecutarComando(comando);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao executar o comando: {ex.Message}");
            }
        }
    }
}