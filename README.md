# Convertor

## Installation

## Useful

```bash
awk 'BEGIN { FS = "\$;" } ; { print $24 }' ./z-assets/input/complete/CPOHYBV.TXT |  sort | uniq
```

## Requirements
```bash
sudo apt install mono-devel
```

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

[MIT](https://choosealicense.com/licenses/mit/)
