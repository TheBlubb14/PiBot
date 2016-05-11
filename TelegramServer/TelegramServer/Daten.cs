using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramServer
{
    public enum BenutzerStatus
    {
        Normal,
        Kaffee_Abfrage,
        GPIO_Abfrage,
        Beenden_Abfrage,
        Freischalten_Status
    }

    public enum BenutzerGruppe
    {
        Gast = 0,
        Benutzer = 1,
        Admin = 2
    }

    public enum SystemZuordnung
    {
        Benutzer,
        Normal = 1,
        System,
        Kaffeemaschine,
    }

    public enum BefehleGast
    {
        Anfrage
    }

    public enum BefehleBenutzer
    {
        Zeit,
        Kaffee
    }

    public enum BefehleAdmin
    {
        Beenden,
        Freischalten
    }

    public enum BefehleAlle
    {
        Anfrage,
        Zeit,
        Kaffee,
        Beenden,
        Freischalten,
        Text
    }

    public struct Benutzer
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Nachricht { get; set; }
        public BenutzerGruppe Gruppe { get; set; }
        public BenutzerStatus Status { get; set; }
    }

    class Daten
    {
    }
}
