dotnet publish -r linux-arm -f net6.0 -o bin/linux-arm/publish --self-contained false
sshpass -p 'bananapi' rsync -auv bin/linux-arm/publish/* pi@bananapi.local:~/boat-battery-monitor/