
using System;
using System.IO;
using System.Security.Cryptography;

namespace autenticacio
{
    class Program
    {
        const string registrats = "logUsuaris.txt"; //el arxiu on aniran els usuaris i contrasenyas dels usuaris
        static void Main(string[] args)
        {
            String eleccio;//variable que ens dona quina opcio del menu tria el usuari.
            //en cas de que el arxiu de usuaris ja existeix, no crea un altre sino que utilitza el ja existent
            if (!(File.Exists(registrats)))
            {
                using (File.Create(registrats))
                {
                }
            }
            //MENU que selecciona la opcio a partir de la variable ELECCIO 
            do
            {
                Console.WriteLine("Triar una de les opcions.");
                Console.WriteLine(" ");
                Console.WriteLine("1 - Registre un usuari.");
                Console.WriteLine("2 - Entrar.");
                Console.WriteLine("0 - Sortir.");
                eleccio = Console.ReadLine();
                switch (eleccio)
                {
                    case "1":
                        registreUsuari();
                        break;
                    case "2":
                        entrarUsuari();
                        break;
                }

            } while (eleccio != "0");
        }


        //metode per fer el reg del usuari.
        static void registreUsuari()
        {

            string[] arx;
            byte[] salt;


            string pass = null;
            string user;
            string control;// aquesta variable ens permet controlar si el usuari introduit al registre ja existeix.
            string Hash;


            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            //demanem el nom d'usuari
            Console.WriteLine("Usuari(no es permeten espais): ");
            user = Console.ReadLine();
            //controlem que no existeixi
            control = buscarUsuari(user);
            if (control != null)
            {
                arx = control.Split(',');

                if (arx[0] == user)
                {
                    Console.WriteLine("El usuari ja existeix.");
                }
            }
            else
            {
                if (!(user.Contains(" ")))
                {
                    //en cas de que el usuari no existeixi demanem la contrasenya
                    Console.WriteLine("Contrasenya: ");
                    ConsoleKeyInfo contr;
                    //permetem el usuari posar la pass fins que presioni el enter
                    do
                    {
                        contr = Console.ReadKey(true);

                        //controlem que cada caracter de la pass sigui diferent a un delete o un enter
                        if (contr.Key != ConsoleKey.Backspace && contr.Key != ConsoleKey.Enter)
                        {
                            pass += contr.KeyChar;
                            Console.Write("*");
                        }

                        // Si es un delete eliminem el ultim caracter de la pass
                        else if (contr.Key == ConsoleKey.Backspace)
                        {
                            //controlem que la pass tingui mes d'un caracter per poder eliminarlo
                            if (pass.Length > 0)
                            {
                                pass.Remove(pass.Length - 1);
                                //l'eliminem.
                                Console.Write("\b \b");
                            }
                        }
                    } while (contr.Key != ConsoleKey.Enter);
                    Console.Write("\n");


                    //realitzem el hash pasantli la contrasenya i la salt
                    Hash = creaHsh(pass, salt);
                    //introduirem al arxiu de usuaris el nom del usuari una "," ,la salt, una altre "," i per ultim el hash de la pass.
                    using (StreamWriter file = new StreamWriter(registrats, true))
                    {
                        file.WriteLine(user + "," + Convert.ToBase64String(salt) + "," + Hash);
                    }
                }
                //en cas de que el usuari intenti posar un espai al nom d'usuari,  no el deixarem avistant de que no es pot.
                else
                {
                    Console.WriteLine("No es permeten espais al nom d'usuari.");
                }
            }
        }
        //metode per realitzar el login del usuari.
        static void entrarUsuari()
        {
            string user;
            string pass = null;
            string info;

            string[] arxiu;
            byte[] saltUsuari;

            //demanem usuari i contrasenya.
            Console.WriteLine("Usuari: ");
            user = Console.ReadLine();

            Console.WriteLine("Contrasenya: ");
            ConsoleKeyInfo contr;

            //realitzem el mateix control de escripptura de contrasenya que hem utilitzat a la creacio del usuari.
            do
            {
                contr = Console.ReadKey(true);
                if (contr.Key != ConsoleKey.Backspace && contr.Key != ConsoleKey.Enter)
                {
                    pass += contr.KeyChar;
                    Console.Write("*");
                }
                else if (contr.Key == ConsoleKey.Backspace)
                {
                    if (pass.Length > 0)
                    {
                        pass.Remove(pass.Length - 1);
                        Console.Write("\b \b");
                    }
                }
            } while (contr.Key != ConsoleKey.Enter);
            Console.Write("\n");
            //busquem el usuari
            info = buscarUsuari(user);
            //si trobem el usuari, 
            if (info != null)
            {
                arxiu = info.Split(',');

                saltUsuari = Convert.FromBase64String(arxiu[1]);
                //convertim a string el hash de la contrasenya i la salt per poder comparar el que ha posat el usuari amb el que hi ha al arxiu de log
                string hashString = creaHsh(pass, saltUsuari);

                //si las pass coincideixen, el usuari entra
                if (hashString == arxiu[2])
                {
                    Console.WriteLine("Has entrat.");
                }
                // sino advertim el usuari que la pass es incorrecte
                else
                {
                    Console.WriteLine("contrasenya incorrecte.");
                }
            }

            //en cas de no trobar-lo advertim al usuari.
            else
            {
                Console.WriteLine("El usuari no ha estat registrat.");
            }
        }


        //creacio del hash.

        static string creaHsh(string pass, byte[] salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(pass, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(32);
            return Convert.ToBase64String(hash);
        }

        //metode per buscar el usuari al arxiu de log si el troba retorna la informacio, sino retorna un null.
        //tenim un try catch per informar que hi ha hagut un problema de lectura del arxiu en cas de que no ens retorni si existeix o no el usuari.

        static string buscarUsuari(string usuari)
        {
            try
            {
                using (StreamReader llegir = new StreamReader(registrats))
                {
                    while (llegir.Peek() > -1)
                    {
                        string line = llegir.ReadLine();
                        string[] user;
                        string userInfo;

                        if (!String.IsNullOrEmpty(line))
                        {
                            user = line.Split(',');
                            if (user[0].Equals(usuari))
                            {
                                userInfo = user[0] + ',' + user[1] + ',' + user[2];
                                return userInfo;
                            }
                        }
                    }
                }
                return null;
            }

            catch
            {
                Console.WriteLine("Error al arxiu.");
                return null;
            }
        }
    }
}
