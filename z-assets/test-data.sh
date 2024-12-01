#!/bin/bash
for filename in input/complete/*.TXT; do
    head -n 10 "$filename" > "input/$(basename "$filename")"
done
