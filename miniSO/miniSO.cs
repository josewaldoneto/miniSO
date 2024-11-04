using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Drawing;
using System.Runtime.CompilerServices;

// Classe usuário
public class Usuario
{
    public string Nome { get; }
    private string SenhaHash { get; }
    private string Salt { get; }
    public Usuario(string nome, string senha)
    {
        Nome = nome;
        Salt = GerarSalt();
        SenhaHash = GerarHashSenha(senha, Salt);
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
            return Convert.ToBase64String(hashBytes);
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
    public void CriarUsuario(string nome, string senha)
    {
        if (usuarios.Exists(u => u.Nome == nome))
            throw new InvalidOperationException("Usuario já existe.");

        usuarios.Add(new Usuario(nome, senha));
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
}

// Definindo os tipos de alocação
public enum TipoAlocacao
{
    FirstFit,
    BestFit,
    WorstFit
}

// Classe de processo
public class Processo
{
    public int Pid { get; }
    public string Nome { get; }
    public int MemoriaAlocada { get; }
    public Processo(int pid, string nome, int memoria)
    {
        Pid = pid;
        Nome = nome;
        MemoriaAlocada = memoria;
    }
}

// Classe do gerenciador de memória
public class GerenciadorMemoria
{
    private List<Processo> processos = new List<Processo>();
    private TipoAlocacao tipoAlocacao;
    private int proxPid = 1;
    public GerenciadorMemoria(TipoAlocacao tipo)
    {
        tipoAlocacao = tipo;
    }
    public Processo CriarProcesso(string nome, int memoriaNecessaria)
    {
        int pid = proxPid++;
        var processo = new Processo(pid, nome, memoriaNecessaria);
        processos.Add(processo);

        Console.WriteLine($"Processo {processo.Pid} criado para '{nome}' com {memoriaNecessaria}MB de memória.");
        return processo;
    }
    public void TerminarProcesso(int pid)
    {
        var processo = processos.Find(p => p.Pid == pid);
        if (processo != null)
        {
            processos.Remove(processo);
            Console.WriteLine($"Processo {processo.Pid} terminado e memória desalocada.");
        }
    }
}

// Implementação dos comandos do Shell
public class MiniShell
{
    private GerenciadorUsuarios gerenciadorUsuarios = new GerenciadorUsuarios();
    private GerenciadorMemoria gerenciadorMemoria = new GerenciadorMemoria(TipoAlocacao.FirstFit);
    public Usuario usuarioAtual;
    public void Iniciar()
    {
        if (!gerenciadorUsuarios.ExistemUsuariosCadastrados())
        {
            Console.WriteLine("Crie um usuário para o primeiro acesso.");
            Console.Write("Nome de usuário: ");
            string nome = Console.ReadLine();
            Console.Write("Senha: ");
            string senha = LerSenha();

            gerenciadorUsuarios.CriarUsuario(nome, senha);
            Console.WriteLine("Usuário criado com sucesso.");

            AutenticarUsuario();
        }
    }
    private void AutenticarUsuario()
    {
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
        if (comando.StartsWith("help"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("help", 1);
            Console.WriteLine($"Processo {processo.Pid} executando comando 'help'");
            MostrarHelp();
            gerenciadorMemoria.TerminarProcesso(processo.Pid);
        }
        else if (comando.StartsWith("criar usuario"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("criar usuario", 10);
            Console.WriteLine($"Processo {processo.Pid} executando comando 'criar usuario'");
            CriarUsuario();
            gerenciadorMemoria.TerminarProcesso(processo.Pid);
        }
        else if (comando.StartsWith("trocar usuario"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("trocar usuario", 10);
            Console.WriteLine($"Processo {processo.Pid} executando comando 'trocar usuario'");
            TrocarUsuario();
            gerenciadorMemoria.TerminarProcesso(processo.Pid);
        }
        else if (comando.StartsWith("listar"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("listar", 1);
            Console.WriteLine($"Processo {processo.Pid} executando comando 'listar'");
            ListarArquivos();
            gerenciadorMemoria.TerminarProcesso(processo.Pid);
        }
        else if (comando.StartsWith("criar arquivo"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("criar arquivo", 10);
            Console.WriteLine($"Processo {processo.Pid} executando comando 'criar arquivo'");
            CriarArquivo(comando);
            gerenciadorMemoria.TerminarProcesso(processo.Pid);
        }
        else if (comando.StartsWith("apagar arquivo"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("apagar arquivo", 10);
            Console.WriteLine($"Processo {processo.Pid} executando comando 'apagar arquivo'");
            ApagarArquivo(comando);
            gerenciadorMemoria.TerminarProcesso(processo.Pid);
        }
        else if (comando.StartsWith("criar diretorio"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("criar diretorio", 10);
            Console.WriteLine($"Processo {processo.Pid} executando comando 'criar diretorio'");
            CriarDiretorio(comando);
            gerenciadorMemoria.TerminarProcesso(processo.Pid);
        }
        else if (comando.StartsWith("apagar diretorio"))
        {
            var processo = gerenciadorMemoria.CriarProcesso("apagar diretorio", 10);
            Console.WriteLine($"Processo {processo.Pid} executando comando 'apagar diretorio'");
            ApagarDiretorio(comando);
            gerenciadorMemoria.TerminarProcesso(processo.Pid);
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
        Console.WriteLine("  listar                    - Lista os arquivos no diretório atual.");
        Console.WriteLine("  criar arquivo <caminho>   - Cria um arquivo no caminho especificado.");
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
    private void ListarArquivos()
    {
        var arquivos = Directory.GetFiles(Directory.GetCurrentDirectory());
        foreach (var arquivo in arquivos)
        {
            Console.WriteLine(Path.GetFileName(arquivo));
        }
    }
    private void CriarArquivo(string comando)
    {
        var partes = comando.Split(' ', 3);
        if (partes.Length < 3)
        {
            Console.WriteLine("Uso: criar arquivo <caminho/arquivo.txt>");
            return;
        }
        string caminhoArquivo = partes[2];
        string conteudo = "placeholder";

        try
        {
            File.WriteAllText(caminhoArquivo, conteudo);
            Console.WriteLine($"Arquivo '{caminhoArquivo}' criado com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar arquivo: {ex.Message}");
        }
    }
    private void ApagarArquivo(string comando)
    {
        var partes = comando.Split(' ', 3);
        if (partes.Length < 3)
        {
            Console.WriteLine("Uso: apagar arquivo <caminho/arquivo.txt>");
            return;
        }

        string caminhoArquivo = partes[2];

        try
        {
            File.Delete(caminhoArquivo);
            Console.WriteLine($"Arquivo '{caminhoArquivo}' apagado com sucesso.");
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

    private void CriarDiretorio(string comando)
    {
        var partes = comando.Split(' ', 3);
        if (partes.Length < 3)
        {
            Console.WriteLine("Uso: criar diretorio <caminho/diretorio>");
            return;
        }

        string caminhoDiretorio = partes[2];

        try
        {
            Directory.CreateDirectory(caminhoDiretorio);
            Console.WriteLine($"Diretório '{caminhoDiretorio}' criado com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar diretório: {ex.Message}");
        }
    }

    private void ApagarDiretorio(string comando)
    {
        var partes = comando.Split(' ', 4);
        bool forcar = partes.Length > 3 && partes[3] == "--force";

        if (partes.Length < 3)
        {
            Console.WriteLine("Uso: apagar diretorio <caminho/diretorio> [--force]");
            return;
        }

        string caminhoDiretorio = partes[2];

        try
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