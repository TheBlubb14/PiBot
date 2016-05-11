using Com.Enterprisecoding.RPI.GPIO;
using Com.Enterprisecoding.RPI.GPIO.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Xml;

namespace TelegramServer
{
    public static class LogikMethoden
    {
        #region Befehle
        public static Benutzer b_Zeit(Api ApiBot, Benutzer benutzer)
        {
            SendeNachricht(ApiBot, benutzer, "Die aktuelle Zeit ist: " + DateTime.Now.ToShortTimeString());
            return benutzer;
        }

        public static Benutzer b_Anfrage(Api ApiBot, Benutzer benutzer)
        {
            // Alle Admins
            List<Benutzer> AdminListe = new List<Benutzer>();
            AdminListe = MySqlKlasse.m_LoadAdmins();

            // Jeder Admin bekommt eine Nachricht, dass der aktuelle Gast freigeschlatet werden möchte
            foreach (Benutzer admin in AdminListe)
            {
                SendeNachricht(ApiBot, admin, "<" + benutzer.ID + "> " + benutzer.Name + " möchte freigeschaltet werden.");
            }

            SendeNachricht(ApiBot, benutzer, "Es wurde eine Anfrage gesendet.");
            return benutzer;
        }

        public static Benutzer b_Kaffee(Api ApiBot, Benutzer benutzer)
        {
            string[][] BefehleArray = new string[][]
            {
                new string[] {"Abbrechen"},
                new string[] { "An", "Aus" }
            };

            // Status ändern
            benutzer.Status = BenutzerStatus.Kaffee_Abfrage;

            SendeNachricht(ApiBot, benutzer, "Verfügbare Befehle:", BefehleArray);

            return benutzer;
        }

        public static Benutzer b_Freischalten(Api ApiBot, Benutzer benutzer)
        {
            List<Benutzer> GaesteListe = new List<Benutzer>();

            string[][] gaesteListe = new string[0][];
            GaesteListe = MySqlKlasse.m_LoadGuest();

            // Alle Gäste aus der Datenbank raussuchen und in einen String[][] packen 
            foreach (Benutzer gast in GaesteListe)
            {
                Array.Resize<string[]>(ref gaesteListe, gaesteListe.Length + 1);
                gaesteListe[gaesteListe.Length - 1] = new string[] { "<" + gast.ID.ToString() + "> " + gast.Name };
            }

            if (gaesteListe.Length > 0)
            {
                // Status ändern
                benutzer.Status = BenutzerStatus.Freischalten_Status;
                SendeNachricht(ApiBot, benutzer, "Gäste:", gaesteListe);
            }
            else
            {
                benutzer.Status = BenutzerStatus.Normal;
                SendeNachricht(ApiBot, benutzer, "Keine Gäste vorhanden.");
            }
            return benutzer;
        }

        public static Benutzer b_Beenden(Api ApiBot, Benutzer benutzer)
        {
            string[][] BefehleArray = new string[][]
            {
                new string[] {"Ja"},
                new string[] {"Nein"}
            };

            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup();
            keyboard.OneTimeKeyboard = true;
            keyboard.ResizeKeyboard = true;
            keyboard.Keyboard = BefehleArray;

            // Status ändern
            benutzer.Status = BenutzerStatus.Beenden_Abfrage;

            SendeNachricht(ApiBot, benutzer, "Programm beenden ?", BefehleArray);

            return benutzer;
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
