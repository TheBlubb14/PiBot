using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace TelegramServer
{
    class MySqlKlasse
    {
        private static MySqlConnection MySqlVerbindungAufbauen()
        {
            try
            {
                MySqlConnection conn = new MySqlConnection("Server=servername;Database=datenbankname;Uid=username;Pwd=passwort;");
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<{0}> Fehler beim MySql Verbindungsaufbau:\n" + ex, DateTime.Now.ToShortTimeString());
                return null;
            }
        }

        public static bool m_SystemLog(string Text, SystemZuordnung Zuordnung)
        {
            Console.WriteLine("<{0}> {1}", DateTime.Now.ToShortTimeString(), Text);

            try
            {
                MySqlConnection conn = MySqlVerbindungAufbauen();
                if (conn == null)
                    return false;

                MySqlCommand cmd = new MySqlCommand("INSERT INTO Systemlogs (Befund, Zuordnung) VALUES (@Text, @Zuordnung)", conn);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("Text", Text);
                cmd.Parameters.AddWithValue("Zuordnung", Zuordnung);

                if (cmd.ExecuteNonQuery() == 1)
                {
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();

                    return true;
                }
                else
                {
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();

                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("<{0}> Es gab eine MySql Fehler:\n" + ex.Message, DateTime.Now.ToShortTimeString());
                return false;
            }
        }

        public static bool m_UserLog(long ID, BefehleAlle Befehl, string Text)
        {
            try
            {
                MySqlConnection conn = MySqlVerbindungAufbauen();
                if (conn == null)
                    return false;

                MySqlCommand cmd = new MySqlCommand("INSERT INTO Userlogs (BenutzerID, Befehl, Text) VALUES (@ID, @Befehl, @Text)", conn);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("ID", ID);
                cmd.Parameters.AddWithValue("Befehl", Befehl.ToString());
                cmd.Parameters.AddWithValue("Text", Text);

                if (cmd.ExecuteNonQuery() == 1)
                {
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();

                    return true;
                }
                else
                {
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();

                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("<{0}> Es gab eine MySql Fehler:\n" + ex.Message, DateTime.Now.ToShortTimeString());
                return false;
            }
        }

        public static bool m_CheckUser(long ID)
        {
            try
            {
                MySqlConnection conn = MySqlVerbindungAufbauen();
                if (conn == null)
                    return false;

                MySqlCommand cmd = new MySqlCommand("SELECT ID FROM Benutzer WHERE ID = @ID", conn);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("ID", ID);

                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();

                if (reader.HasRows)
                {
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();

                    return true;
                }
                else
                {
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();
                    Console.WriteLine("<{0}> Benutzer ist nicht vorhanden.", DateTime.Now.ToShortTimeString());

                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("<{0}> Es gab eine MySql Fehler:\n" + ex.Message, DateTime.Now.ToShortTimeString());
                return false;
            }
        }

        public static Benutzer m_LoadUser(long ID)
        {
            Benutzer benutzer = default(Benutzer);

            try
            {
                MySqlConnection conn = MySqlVerbindungAufbauen();
                if (conn == null)
                    return default(Benutzer);


                MySqlCommand cmd = new MySqlCommand("SELECT ID, Name, Gruppe FROM Benutzer WHERE ID = @ID", conn);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("ID", ID);

                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();

                benutzer.ID = reader.GetInt64(0);
                benutzer.Name = reader.GetString(1);
                benutzer.Gruppe = (BenutzerGruppe)Enum.Parse(typeof(BenutzerGruppe), reader["Gruppe"].ToString());

            }
            catch (Exception ex)
            {
                Console.WriteLine("<{0}> Es gab eine MySql Fehler:\n" + ex.Message, DateTime.Now.ToShortTimeString());
                return default(Benutzer);
            }

            return benutzer;
        }

        public static List<Benutzer> m_LoadGuest()
        {
            List<Benutzer> gaeste = new List<Benutzer>();

            try
            {
                MySqlConnection conn = MySqlVerbindungAufbauen();
                if (conn == null)
                    return gaeste;


                MySqlCommand cmd = new MySqlCommand("SELECT ID, Name, Gruppe FROM Benutzer WHERE Gruppe = @Gruppe", conn);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("Gruppe", BenutzerGruppe.Gast);

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    gaeste.Add(new Benutzer()
                    {
                        ID = reader.GetInt64(0),
                        Name = reader.GetString(1),
                        Gruppe = (BenutzerGruppe)Enum.Parse(typeof(BenutzerGruppe),reader["Gruppe"].ToString())
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("<{0}> Es gab eine MySql Fehler:\n" + ex.Message, DateTime.Now.ToShortTimeString());
                return gaeste;
            }

            return gaeste;
        }

        public static List<Benutzer> m_LoadAdmins()
        {
            List<Benutzer> admins = new List<Benutzer>();

            try
            {
                MySqlConnection conn = MySqlVerbindungAufbauen();
                if (conn == null)
                    return admins;


                MySqlCommand cmd = new MySqlCommand("SELECT ID, Name, Gruppe FROM Benutzer WHERE Gruppe = @Gruppe", conn);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("Gruppe", BenutzerGruppe.Admin);

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    admins.Add(new Benutzer()
                    {
                        ID = reader.GetInt64(0),
                        Name = reader.GetString(1),
                        Gruppe = (BenutzerGruppe)Enum.Parse(typeof(BenutzerGruppe), reader["Gruppe"].ToString())
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("<{0}> Es gab eine MySql Fehler:\n" + ex.Message, DateTime.Now.ToShortTimeString());
                return admins;
            }

            return admins;
        }

        public static bool m_CreateUser(long ID, string Name, BenutzerGruppe Gruppe)
        {
            try
            {
                MySqlConnection conn = MySqlVerbindungAufbauen();
                if (conn == null)
                    return false;


                MySqlCommand cmd = new MySqlCommand("INSERT INTO Benutzer (ID, Name, Gruppe) VALUES (@ID, @Name, @Gruppe)", conn);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("ID", ID);
                cmd.Parameters.AddWithValue("Name", Name);
                cmd.Parameters.AddWithValue("Gruppe", Gruppe);

                if (cmd.ExecuteNonQuery() == 1)
                {
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();

                    m_SystemLog("Ein neuer Benutzer wurde angelegt", SystemZuordnung.Benutzer);

                    return true;
                }
                else
                {
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();

                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("<{0}> Es gab eine MySql Fehler:\n" + ex.Message, DateTime.Now.ToShortTimeString());
                return false;
            }
        }

        public static bool m_UnlockGuest(long ID)
        {
            try
            {
                MySqlConnection conn = MySqlVerbindungAufbauen();
                if (conn == null)
                    return false;


                MySqlCommand cmd = new MySqlCommand("UPDATE Benutzer SET Gruppe = @Gruppe WHERE ID = @ID", conn);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("Gruppe", BenutzerGruppe.Benutzer);
                cmd.Parameters.AddWithValue("ID", ID);

                if (cmd.ExecuteNonQuery() == 1)
                {
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();

                    m_SystemLog("Ein Gast wurde freigeschaltet", SystemZuordnung.Benutzer);

                    return true;
                }
                else
                {
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();

                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("<{0}> Es gab eine MySql Fehler:\n" + ex.Message, DateTime.Now.ToShortTimeString());
                return false;
            }

        }
    }
}
