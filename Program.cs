using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Npgsql;
using Newtonsoft.Json;

class Program
{
    static async Task Main(string[] args)
    {
        // URL para colsuta e lista para armazenar os dados de usuários
        string apiUrl = "https://randomuser.me/api/?results=5";
        List<User> users = new List<User>();

        // Criação do objeto HttpClient
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Faz uma solicitação GET à API
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                // Verifica se a requisição teve sucesso 
                if (response.IsSuccessStatusCode)
                {
                    // ler o conteudo como string e converter o JSON para objeto UserResult
                    string json = await response.Content.ReadAsStringAsync();

                    UserResult result = JsonConvert.DeserializeObject<UserResult>(json);

                    // Verifica se retornaram resultados - adicionar usurios a lista e conectar com o banco de dados 
                    if (result != null && result.results.Length > 0)
                    {
                        users.AddRange(result.results);

                        string connectionString = "Host=localhost;Port=5432;Database=random_api_bd;Username=postgres;Password=Bd123@;";

                        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                        {
                            await connection.OpenAsync();

                            // inseri dados na tabela do banco de dados
                            foreach (User user in users)
                            {
                                string name = $"{user.name.first} {user.name.last}"; //concatenando o prim nome e o sobrenome

                                string sql = "INSERT INTO users (name, age, email, country) VALUES (@Name, @Age, @Email, @Country)";
                                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                                {
                                    command.Parameters.AddWithValue("@Name", name);
                                    command.Parameters.AddWithValue("@Age", user.dob.age);
                                    command.Parameters.AddWithValue("@Email", user.email);
                                    command.Parameters.AddWithValue("@Country", user.location.country);

                                    await command.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        Console.WriteLine("Dados inseridos com sucesso no banco de dados.");
                    }
                    else
                    {
                        Console.WriteLine("Nenhum usuário retornado.");
                    }
                }
                else
                {
                    Console.WriteLine("Falha na solicitação HTTP.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro: {ex.Message}");
            }

        }
    }
}

// Classes para converter os JSON
public class UserResult
{
    public User[] results { get; set; }
}

public class User
{
    public Name name { get; set; }
    public Dob dob { get; set; }
    public string email { get; set; }
    public Location location { get; set; }
    
}

public class Name
{
    public string first { get; set; }
    public string last { get; set; }

}

public class Dob
{
    public int age { get; set; }
}

public class Location
{
    public string country { get; set; }
   
}
