#!/bin/bash

if [ -z "$1" ]
  then
    echo "> no port supplied"
    exit
fi

sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport $1 -j ACCEPT
sudo netfilter-persistent save