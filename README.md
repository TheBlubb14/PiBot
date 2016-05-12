# PiBot

PiBot ist ein Schulprojekt, bei dem man durch Telegram einen TelegramBot ansprechen kann.
Dieser kann verschiedene Funktionen haben und hat für das Schulprojekt eine Steckdose mithilfe eines Relais über die GPIO geschaltet. 

-- Funktionen --

- Schalten verschiedener GPIO eines Raspberry Pi (WiringPi)
- Rechtesystem (Gast, Benutzer, Admin) und Logging mithilfe einer MySql Datenbank


-- zum Erstellen --

- MySql Datenbank mit teledp.sql als Struktur aufsetzen
- in MySqlKlasse.cs in der Methode "MySqlVerbindungAufbauen" die MySql Serverdaten eintragen
- in Program.cs in dem Task "ServerAufgaben" bei ApiBot = new ApiBot("XXXX") TelegramBot Api einfügen
