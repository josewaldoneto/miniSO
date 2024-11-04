using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;

public class Usuario
{
	public string Nome { get; }
    private string SenhaHash { get; }
    private string Salt { get; }

    private static string GerarSalt()
    {
        byte[] saltBytes = new byte[16];
    }

    public Usuario(string nome, string senha)
	{
        Nome = nome;
        Salt = GerarSalt();
        SenhaHash = GerarHashSenha(senha, Salt);
	}
}

public static Main()
{
    Console.WriteLine(byte[16]);
}
