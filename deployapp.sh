#!/bin/bash

# Docker build command
docker build -t incharge:latest .

# Docker save command
docker save -o "C:\Users\krist\Documents\DockerTempImages\incharge" incharge:latest

# Kopiera till Ubuntu-server
scp "C:\Users\krist\Documents\DockerTempImages\incharge" krister@192.168.68.140:/home/krister/DockerImages/

# SSH till Ubuntu-server
ssh -t krister@192.168.68.140 << EOF

# Stoppa och ta bort container (if exists)
CONTAINER_ID=\$(docker ps -q --filter "name=incharge-app")
if [ -n "\$CONTAINER_ID" ]; then
    docker stop \$CONTAINER_ID
    docker rm \$CONTAINER_ID
fi

# Ta bort Docker image (if exists)
IMAGE_ID=\$(docker images -q incharge:latest)
if [ -n "\$IMAGE_ID" ]; then
    docker rmi \$IMAGE_ID
fi

# Ladda Docker image från fil
docker load -i /home/krister/DockerImages/incharge

# Ta bort filen på servern efter att den har laddats in i Docker
rm /home/krister/DockerImages/incharge

# Kör ny Docker container
docker run --name incharge-app --restart always -d -p 32777:8080 incharge:latest

EOF

# Ta bort Docker image och filen från Windows
rm "C:\Users\krist\Documents\DockerTempImages\incharge"
docker rmi incharge:latest
