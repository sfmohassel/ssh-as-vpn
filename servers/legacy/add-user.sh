#!/bin/bash

if [ -z "$1" ]
  then
    echo "> no username supplied"
    exit
fi

GROUP="ninja"
if grep -q ${GROUP} /etc/group
then
  echo "${GROUP} exists"
else
  sudo groupadd ${GROUP}
fi

if id "$1" &>/dev/null; then
  echo "> $1 found. will reset password"
else
  echo "> creating an account for $1 and set a random password"
  sudo useradd "$1"
fi


PASS=$(openssl rand -hex 4)
sudo usermod --password $(openssl passwd -1 ${PASS}) "$1"

if getent group ${GROUP} | grep -q "\b$1\b"; then
  echo "> $1 is already a ${GROUP} ;)"
else
  echo "> $1 is becoming a ${GROUP}"
  sudo usermod -a -G "${GROUP}" "$1"
fi

echo ">>>>>>>>>>>>>>>>>>>>>>"
echo "Username: $1"
echo "Password: ${PASS}"
echo "<<<<<<<<<<<<<<<<<<<<<<"