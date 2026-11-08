# Labirinta navigācijas eksperiments
Pētāmais jautājums: Vai krāsu filtrs ietekmē navigācijas laiku virtuālā labirintā?
Eksperiments sastāv no 3 daļām (izmēģinājuma, eksperimenta un demogrāfiskās aptaujas daļas) sekojoši:

# Izmēģinājuma daļa
Dalībniekam tiek attēlota izmēģinājuma telpa VR kontroļu apgūšanai. Šajā telpā ir neierobežots atrašanās laiks, kā arī tajā atrodas istaba ar objektu, kas kalpo kā piemērs ar istabām, ar ko dalībniekam nāksies saskarties eksperimenta laikā.
Kad dalībnieks ir apguvis kontroļu izmantošanu, tad var tikt uzsākts eksperiments, nospiežot pogu "Sākt".

# Eksperimenta daļa
Šī daļa tiek atkārtota trīs reizes, mainoties istabu ieeju vietai, objektiem istabās un krāsu filtram (zils, sarkans, bez filtra).
Dalībniekam tiek attēlota telpa ar 9 istabām. Ir neierobežots laiks, lai apgūtu objektu izveitojumu tajās, staigājot pa virtuālo labirintu.
Pēc objektu atrašanās vietu apguves dalībnieks atgriežas sākuma pozīcijā un spiež pogu "Sākt". Dalībniekam tiek attēlots teksts ar objektu, kas jāatrod (kopā jāatrod 3 objekti). Ieejot istabā, kurā ir meklējamais objekts, tiek attēlots nākamais atrodamais objekts. Paskatoties uz kreiso roku, dalībniekam tiek parādīts teksts ar objektu, kas jāatrod (gadījumiem, ja tiek aizmirsts). Pēc visu objektu atrašanas dalībnieks tiek pārvietots telpā, kurā dod atbildes uz jautājumiem par pašreizējām emocijām. Pēc jautājumu aizpildes dalībnieks atkal atkārto istabu izpēti un objektu meklēšanu.  

# Demogrāfijas aptaujas daļa

Eksperimenta nobeigumā dalībnieks atbild uz demogrāfijas jautājumiem.

# Kā uzstādīt projektu
1. Lejupielādējiet Godot 4.x.x dzini ar **.NET** atbalstu (**Godot Engine – .NET**): https://godotengine.org/download/
2. Lejupielādējiet šo projektu jūsu izvēlētā direktorijā izmantojot *git clone ...*
3. Atveriet Godot dzini un nospiediet pogu "Import", atveriet jūsu direktoriju, kurā atrodas šis projekts un nospiediet uz **project.godot**
4. Projektam "Maze experiment" vajadzētu parādīties galvenajā dziņa logā, spiediet uz tā
5. Tiks atvērts šis projekts

# Kā palaist uz Meta Quest 2/3 brillēm
Lai palaistu projektu uz virtuālās realitātes brillēm, nepieciešams lejupielādēt android ADB rīku, ieslēgt *izstrādātāja režīmu* brillēm, savienot brilles ar datoru, izmantojot USB-C vadu.
Pēc tā ir iespējams Godot dzinī nospiest pogu augšējā labajā pusē "remote deploy" un izvēlēties jūsu Quest brilles. Pēc pogas nospiešanas projekts tiks sakompilēts kā *android* aplikācija un tā tiks palaista uz jūsu brillēm.

# Ierobežojumi

Projektu ir iespējams palaist tikai uz virtuālās realitātes brillēm
