using Com.Enterprisecoding.RPI.GPIO;
using Com.Enterprisecoding.RPI.GPIO.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramServer
{
    public static class LogikStatus
    {
        private static int wiringPiCheck = -1;

        public static void InitWiringPi()
        {
            // Darf nur 1x pro Programm Instantz ausgeführt werden
            try
            {
                wiringPiCheck = WiringPi.Core.Setup();
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("<{0}> Pin-Error: Computer ist kein Raspberry", DateTime.Now.ToShortTimeString());
            }
        }

        #region Status
        public static Benutzer s_Kaffee(Api ApiBot, Benutzer benutzer)
        {
            if (wiringPiCheck == -1)
            {
                SendeNachricht(ApiBot, benutzer, "Auf die GPIO Pins kann nicht zugegriffen werden!");
                MySqlKlasse.m_SystemLog("WiringPi initialisierung fehlgeschlagen", SystemZuordnung.System);

                benutzer.Status = BenutzerStatus.Normal;
                return benutzer;

            }


            if (benutzer.Nachricht.ToUpper() == "AN")
            {
                WiringPi.Core.PinMode(0, PinMode.Output);
                WiringPi.Core.DigitalWrite(0, DigitalValue.Low);
                WiringPi.Core.DigitalWrite(0, DigitalValue.High);
                Console.WriteLine("pin aktiviert");

                MySqlKlasse.m_SystemLog("Kaffeemaschine angeschaltet", SystemZuordnung.Kaffeemaschine);

                SendeNachricht(ApiBot, benutzer, "Kaffeemaschine wurde angeschaltet.");
            }
            else if (benutzer.Nachricht.ToUpper() == "AUS")
            {
                WiringPi.Core.PinMode(0, PinMode.Output);
                WiringPi.Core.DigitalWrite(0, DigitalValue.Low);
                Console.WriteLine("pin deaktiviert");

                MySqlKlasse.m_SystemLog("Kaffeemaschine ausgeschaltet", SystemZuordnung.Kaffeemaschine);

                SendeNachricht(ApiBot, benutzer, "Kaffeemaschine wurde ausgeschaltet.");
            }

            benutzer.Status = BenutzerStatus.Normal;

            return benutzer;
        }

        public static Benutzer s_Beenden(Api ApiBot, Benutzer benutzer)
        {
            if (benutzer.Nachricht.ToUpper() == "JA")
            {
                MySqlKlasse.m_SystemLog("System wird heruntergefahren", SystemZuordnung.System);
                SendeNachricht(ApiBot, benutzer, "System wird heruntergefahren.");
                Environment.Exit(0);
            }

            benutzer.Status = BenutzerStatus.Normal;

            return benutzer;
        }

        public static Tuple<Benutzer, List<Benutzer>> s_Freischalten(Api ApiBot, Benutzer benutzer, List<Benutzer> BenutzerListe)
        {
            // Enthält die Nachricht "<" + neun Zahlen 0-9 + ">" (das ist die UserID)
            Match match = Regex.Match(benutzer.Nachricht, @"<([0-9]{9})>");

            if (match.Success)
            {
                // Herausholen der GastID aus der gefundenen Sequenz
                long gastID = Convert.ToInt64(match.Value.Replace("<", "").Replace(">", ""));
                if (MySqlKlasse.m_UnlockGuest(gastID))
                {
                    // Sollte der freigeschaltete Benutzer schon eine Session haben, so wird diese aktualisiert
                    if (BenutzerListe.Contains(BenutzerListe.Where(x => x.ID == gastID).FirstOrDefault()))
                    {
                        // alten "Gast" entfernen
                        BenutzerListe.Remove(BenutzerListe.Where(x => x.ID == gastID).First());

                        // Neue Session für den Benutzer erstellen
                        BenutzerListe.Add(MySqlKlasse.m_LoadUser(gastID));
                    }

                    SendeNachricht(ApiBot, benutzer, "Gast wurde freigeschaltet.");
                }
                else
                {
                    SendeNachricht(ApiBot, benutzer, "Konnte Gast nicht freischalten.");
                }
            }

            benutzer.Status = BenutzerStatus.Normal;

            return new Tuple<Benutzer, List<Benutzer>>(benutzer, BenutzerListe);
        }

        public static void s_Text(Benutzer benutzer)
        {
            if (!string.IsNullOrEmpty(benutzer.Nachricht))
            {
                MySqlKlasse.m_UserLog(benutzer.ID, BefehleAlle.Text, benutzer.Nachricht);
            }
        }
        #endregion 

        private static async void SendeNachricht(Api ApiBot, Benutzer benutzer, string Nachricht, string[][] keyboard = null)
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
