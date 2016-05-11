using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Com.Enterprisecoding.RPI.GPIO;
using Com.Enterprisecoding.RPI.GPIO.Enums;
using System.Reflection;
using System.Linq.Expressions;

namespace TelegramServer
{
    class Program
    {
        private static Api ApiBot;

        static void Main(string[] args)
        {
            LogikStatus.InitWiringPi();

            while (true)
            {
                try
                {
                    //Die Methode ServerAufgaben wird ausgeführt, sobald er eine Aufgabe bekommt
                    ServerAufgaben().Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("<{0}> Systemfehler:\n" + ex, DateTime.Now.ToShortTimeString());
                    MySqlKlasse.m_SystemLog("Systemfehler: " + ex, SystemZuordnung.System);
                    Thread.Sleep(30000);
                }
            }
        }




        static async Task ServerAufgaben()
        {
            // Der Bot wird initialisiert
            ApiBot = new Api("XXXX");
            var BotInfo = ApiBot.GetMe().Result;

            // Die lokale Benutzerliste wird angelegt, um die Netzverbindung nicht auslasten zu müssen
            List<Benutzer> BenutzerListe = new List<Benutzer>();
            Benutzer aktuellerBenutzer = default(Benutzer);

            // Liste, die alle Befehle als Methoden enthält
            // Api(ApiBot - input), Benutzer(aktuellerBenutzer - input), Benutzer(aktuellerBenutzer - output)
            List<Func<Api, Benutzer, Benutzer>> BefehleListeAlle = new List<Func<Api, Benutzer, Benutzer>>();

            // Alle Methoden aus der LogikMethoden Klasse bekommen
            MethodInfo[] methoden = typeof(LogikMethoden).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            // Alle Methoden aus der LogikMethoden Klasse in die BefehlsListe schreiben
            foreach (MethodInfo methode in methoden)
            {
                BefehleListeAlle.Add((Func<Api, Benutzer, Benutzer>)Delegate.CreateDelegate(typeof(Func<Api, Benutzer, Benutzer>), methode));
            }

            // Meldung, ob der Server gestartet wurde
            MySqlKlasse.m_SystemLog("Der Server " + BotInfo.Username + " wurde gestartet\n", SystemZuordnung.System);

            // Wir fangen bei der ersten neuen Nachricht an
            int offset = 0;

            // Eine Schleife, welche durchgehend den Server nach neuen Nachrichten abfragt
            while (true)
            {
                var updates = await ApiBot.GetUpdates(offset);

                foreach (Update update in updates)
                {

                    // Nachricht des Benutzers ausgeben
                    Console.WriteLine("<{0}> | <{1}> {2} {3} | Nachricht: {4}", DateTime.Now.ToShortTimeString(), update.Message.Chat.Id, update.Message.Chat.FirstName, update.Message.Chat.LastName, update.Message.Text);

                    #region Session
                    // Überprüfen ob der User noch keine lokale Session hat
                    if (BenutzerListe.Where(x => x.ID == update.Message.Chat.Id).FirstOrDefault().ID == 0)
                    {
                        // Überprüfen ob der User schon auf der Datenbank existiert
                        if (!MySqlKlasse.m_CheckUser(update.Message.Chat.Id))
                        {
                            // Benutzer ist nicht vorhanden -> Erstelle neuen Benutzer
                            MySqlKlasse.m_CreateUser(update.Message.Chat.Id, update.Message.Chat.LastName + "_" + update.Message.Chat.FirstName, BenutzerGruppe.Gast);
                            Console.WriteLine("<{0}> Benutzer wurde auf Datenbank erstellt", DateTime.Now.ToShortTimeString());
                        }

                        // Neue Session für den Benutzer erstellen
                        BenutzerListe.Add(MySqlKlasse.m_LoadUser(update.Message.Chat.Id));
                        Console.WriteLine("<{0}> Neue BenutzerSession für {1} erstellt", DateTime.Now.ToShortTimeString(), update.Message.Chat.FirstName + " " + update.Message.Chat.LastName);
                    }

                    // Benutzer, der die Nachricht geschickt hat wird aus der Benutzerliste gefischt und als aktueller Benutzer gespeichert
                    aktuellerBenutzer = BenutzerListe.Where(x => x.ID == update.Message.Chat.Id).FirstOrDefault();
                    aktuellerBenutzer.Nachricht = update.Message.Text;
                    #endregion

                    #region Status
                    if (aktuellerBenutzer.Status == BenutzerStatus.Kaffee_Abfrage)
                    {
                        aktuellerBenutzer = LogikStatus.s_Kaffee(ApiBot, aktuellerBenutzer);
                    }
                    else if (aktuellerBenutzer.Status == BenutzerStatus.Beenden_Abfrage)
                    {
                        aktuellerBenutzer = LogikStatus.s_Beenden(ApiBot, aktuellerBenutzer);
                    }
                    else if (aktuellerBenutzer.Status == BenutzerStatus.Freischalten_Status)
                    {
                        Tuple<Benutzer, List<Benutzer>> result = LogikStatus.s_Freischalten(ApiBot, aktuellerBenutzer, BenutzerListe);
                        aktuellerBenutzer = result.Item1;
                        BenutzerListe = result.Item2;
                    }
                    else
                    {
                        LogikStatus.s_Text(aktuellerBenutzer);
                    }
                    #endregion 

                    #region Befehle
                    if (aktuellerBenutzer.Nachricht.ToUpper().StartsWith("/HILFE") || aktuellerBenutzer.Nachricht.ToUpper().StartsWith("/START"))
                    {
                        #region Hilfe
                        // Alle Befehle werden in einem Array gespeichert
                        List<string> befehle = new List<string>();
                        string[][] BefehleArray = new string[0][];


                        if (aktuellerBenutzer.Gruppe == BenutzerGruppe.Gast)
                        {
                            // Alles GastBefehle
                            List<string> gastBefehle = Enum.GetNames(typeof(BefehleGast)).ToList<string>();
                            befehle.AddRange(gastBefehle);
                        }
                        else if (aktuellerBenutzer.Gruppe == BenutzerGruppe.Benutzer)
                        {
                            // Alle BenutzerBefehle
                            List<string> benutzerBefehle = Enum.GetNames(typeof(BefehleBenutzer)).ToList<string>();
                            befehle.AddRange(benutzerBefehle);
                        }
                        else if (aktuellerBenutzer.Gruppe == BenutzerGruppe.Admin)
                        {
                            // Alle AdminBefehle
                            List<string> benutzerBefehle = Enum.GetNames(typeof(BefehleBenutzer)).ToList<string>();
                            List<string> adminBefehle = Enum.GetNames(typeof(BefehleAdmin)).ToList<string>();

                            // Zusammenfügen der Benutzer - und Adminbefehle
                            befehle.AddRange(benutzerBefehle);
                            befehle.AddRange(adminBefehle);
                        }

                        #region Aus Array 2d Array mit einer Länge von je 3 Elementen machen
                        // Indexer
                        int iaussen = -1;
                        int iinnen = 0;

                        for (int iallgemein = 0; iallgemein < befehle.Count; iallgemein++)
                        {
                            if (iallgemein % 3 == 0)
                            {
                                Array.Resize<string[]>(ref BefehleArray, BefehleArray.Length + 1);
                                iaussen++;
                                iinnen = 0;
                                BefehleArray[iaussen] = new string[] { "", "", "" };
                            }

                            BefehleArray[iaussen][iinnen] = "/" + befehle[iallgemein].ToString();
                            iinnen++;
                        }
                        #endregion 


                        SendeNachricht(aktuellerBenutzer, "Verfügbare Befehle:", BefehleArray);
                        #endregion
                    }
                    else if (aktuellerBenutzer.Gruppe == BenutzerGruppe.Gast)
                    {
                        #region Gast
                        List<string> befehle = Enum.GetNames(typeof(BefehleGast)).ToList<string>();

                        string befehl = befehle.Where(x => aktuellerBenutzer.Nachricht.ToUpper().StartsWith("/" + x.ToUpper())).FirstOrDefault();

                        // Raussuchen der Methode aus der BefehleListeAlle wo der Methodenname = "b_" + der eingegebene erkannte Befehl des Benutzers
                        Func<Api, Benutzer, Benutzer> aktMethode = BefehleListeAlle.Where(x => x.Method.Name == "b_" + befehl).FirstOrDefault();

                        // Wenn der Befehl gefunden wurde
                        if (aktMethode != null)
                        {
                            aktuellerBenutzer = aktMethode(ApiBot, aktuellerBenutzer);
                        }
                        #endregion 
                    }
                    else if (aktuellerBenutzer.Gruppe == BenutzerGruppe.Benutzer)
                    {
                        #region Benutzer
                        List<string> befehle = Enum.GetNames(typeof(BefehleBenutzer)).ToList<string>();

                        string befehl = befehle.Where(x => aktuellerBenutzer.Nachricht.ToUpper().StartsWith("/" + x.ToUpper())).FirstOrDefault();

                        // Raussuchen der Methode aus der BefehleListeAlle wo der Methodenname = "b_" + der eingegebene erkannte Befehl des Benutzers
                        Func<Api, Benutzer, Benutzer> aktMethode = BefehleListeAlle.Where(x => x.Method.Name == "b_" + befehl).FirstOrDefault();

                        // Wenn der Befehl gefunden wurde
                        if (aktMethode != null)
                        {
                            aktuellerBenutzer = aktMethode(ApiBot, aktuellerBenutzer);
                        }
                        #endregion 
                    }
                    else if (aktuellerBenutzer.Gruppe == BenutzerGruppe.Admin)
                    {
                        #region Admin
                        List<string> befehle = new List<string>();
                        List<string> benutzerBefehle = Enum.GetNames(typeof(BefehleBenutzer)).ToList<string>();
                        List<string> adminBefehle = Enum.GetNames(typeof(BefehleAdmin)).ToList<string>();

                        // Zusammenfügen der Benutzer- und Adminbefehle
                        befehle.AddRange(benutzerBefehle);
                        befehle.AddRange(adminBefehle);

                        // Erkennt den gesendeten Befehl vom Benutzer und überprüft ob der Befehl für den Benutzer exsistiert
                        // wenn der Befehl für den Benutzer exsistiert, dann wird der Befehl zurück gegeben, sonst null
                        string befehl = befehle.Where(x => aktuellerBenutzer.Nachricht.ToUpper().StartsWith("/" + x.ToUpper())).FirstOrDefault();


                        // Raussuchen der Methode aus der BefehleListeAlle wo der Methodenname = "b_" + der eingegebene erkannte Befehl des Benutzers
                        Func<Api, Benutzer, Benutzer> aktMethode = BefehleListeAlle.Where(x => x.Method.Name == "b_" + befehl).FirstOrDefault();

                        // Wenn der Befehl gefunden wurde
                        if (aktMethode != null)
                        {
                            aktuellerBenutzer = aktMethode(ApiBot, aktuellerBenutzer);
                        }
                        #endregion 
                    }
                    #endregion

                    // Aktueller Benutzer wird in die Benutzerliste, mit dem aktualisierten Status, geschrieben
                    int index = BenutzerListe.IndexOf(BenutzerListe.Where(x => x.ID == aktuellerBenutzer.ID).FirstOrDefault());
                    BenutzerListe[index] = aktuellerBenutzer;

                    //Nachrichtenwert wird erhöht
                    offset = update.Id + 1;
                }
            }
        }

        private static async void SendeNachricht(Benutzer benutzer, string Nachricht, string[][] keyboard = null)
        {
            ReplyKeyboardMarkup Keyboard = null;

            if (keyboard != null)
            {
                Keyboard = new ReplyKeyboardMarkup();
                Keyboard.OneTimeKeyboard = true;
                Keyboard.ResizeKeyboard = true;
                Keyboard.Keyboard = keyboard;
            }

            Console.WriteLine("<{0}> Sende Nachricht: " + Nachricht, DateTime.Now.ToShortTimeString());
            await ApiBot.SendTextMessage(benutzer.ID, Nachricht, false, 0, Keyboard);
        }
    }
}