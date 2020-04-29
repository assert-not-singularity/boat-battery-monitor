dotnet publish -r linux-arm -f netstandard2.0 -o bin/linux-arm/publish --self-contained false
sshpass -p 'raspberry' rsync -auv bin/linux-arm/publish/* pi@raspberrypi.local:~/boat-battery-monitor/