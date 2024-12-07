# Convertor

## Installation

## Useful

```bash
awk 'BEGIN { FS = "\$;" } ; { print $24 }' ./z-assets/input/complete/CPOHYBV.TXT |  sort | uniq
xml sel -t -c "/S5Data/SkladovyDokladList/SkladovyDoklad[4]" S6_SklDokl.xml > failed.xml
```

## Requirements
```bash
sudo apt install mono-devel
```

## Contributing

## Important
final version S4_Agenda_LIPA is on VM local
final verison S4_System is on PEMA MONEY

## Import Money
1. copy exportsklad to VM local Downloads
2. run export 
3. copy content of exportsklad/output to z-assets/input/complete-lipa
4. copy content of z-assets/input/complete-lipa to z-assets/import
5. clear directory z-assets/output
6. replicate System DB from PEMA to LIPA - script
7. backup System DB from LIPA
8. move System DB to VM-DEV backup/final_lipa
9.  restore DBs from backup/final_lipa - script
10. Export S0_IDs copy to z-assets/output/
11. Run money-import 
12. Import S2_Adresar, S3_Katalog
13. Export S0_IDs copy to z-assets/output/
14. Run money-import
15. Import S6_, S7_
16. Run script update-data-po-importu.sql
