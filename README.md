# Projeto Final - Shell para MiniSO

```
MiniSO/
    ├── miniSO.cs - Código
    └── bin/
        └── Debug/
            └── net8.0/ - Diretório onde será criado/apagado/listado arquivos ao usar o shell
                └── miniSO.exe
```
---
- Todas as operações do shell serão persistida no diretório local onde ele executa!

- TODA a execução de comando, incluindo a criação de usuário no primeiro acesso deve ser executada mediante a criação de um novo processo de SO e seu pid deve ser impresso antes de qualquer mensagem para identificá-lo 

- CADA processo deve usar algum dos algoritmos de alocação de memória que estudamos (best fit, worst fit e first fit), alocando a memória para sua execução e desalocando-a ao fim.

## Segurança:

### Primeiro acesso ao shell (caso não haja usuário cadastrado):

- Caso não exista nenhum usuário cadastrado no MiniSO, será solicitado no shell, a criação de um usuário e com senha.
A senha deve ser salva utilizando um salt e em hash (SHA-512), como foi feito no exercício de segurança.
- Caso haja pelo menos 1 usuário cadastrado, o shell solicitará usuário e senha para login. A senha não deve aparecer enquanto o usuário a digita! (Pode ficar com asteriscos ou sem nada no lugar)
- Caso o último usuário do MiniSO seja excluído, deve ser executado o passo 1 assim que ele seja apagado e para cada nova execução

## Operações que o shell DEVE ter:

### Nome do Comando: "listar dir1"
- Cria um novo processo e aloca memória para ele e executa: o equivalente ao ls do linux (dir do prompt de comando / power shell do windows). 
  - Lista o conteúdo de diretórios e arquivos do dir1. 
  - Caso não tenha parâmetro depois do listar, lista apenas o diretório corrente.
### Nome do Comando: "criar arquivo dir1/arquivo1.txt" 
- Cria um novo processo e aloca memória para ele e executa: cria o arquivo1.txt dentro do diretório dir1 com conteúdo aleatório. 
  - Caso não tenha "dir1/" antes do nome do arquivo no comando, ele será criado no diretório corrente.
### Nome do Comando: "apagar arquivo dir1/arquivo1.txt" 
- Cria um novo processo e aloca memória para ele e executa: apaga o arquivo1.txt do diretório dir1. 
  - Caso não tenha "dir1/" antes do nome do arquivo no comando, ele será apagado no diretório corrente.
### Nome do Comando: "criar diretorio dir1/dir2" 
- Cria um novo processo e aloca memória para ele e executa: cria o diretório dir2 vazio dentro do diretório dir1. 
  - Caso não tenha "dir1/" antes do diretório no comando, ele será criado no diretório corrente.
### Nome do Comando: "apagar diretorio dir1/dir2" 
- Cria um novo processo e aloca memória para ele e executa: apaga o dir1 do diretório corrente, CASO ESTEJA VAZIO. 
  - Caso não tenha "dir1/" antes do diretório no comando, ele será apagado a partir do diretório corrente.
### Nome do Comando: "apagar diretorio dir1/dir2 --force" 
- Cria um novo processo e aloca memória para ele e executa: apaga o dir1 do diretório corrente, MESMO QUE TENHA ARQUIVOS OU DIRETÓRIOS.  
  - Caso não tenha "dir1/" antes do diretório no comando, ele será apagado a partir do diretório corrente.

---
### Deve ser salvo no arquivo e no diretório quem é o dono deles, para evitar que outro usuário cadastrado acesso sem permissão

### Caso o usuário 1 tenho criado o arquivo ou diretório, o usuário 2 não terá acesso a ele e receberá um erro avisando que ele não tem permissão

