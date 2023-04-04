#!/bin/bash

# sudo ufw allow ssh
# sudo ufw enable

GROUP="ninja"
if grep -q ${GROUP} /etc/group
then
  echo "${GROUP} exists"
else
  sudo groupadd ${GROUP}
fi

CURRENT_GROUP=$(id -g -n)

sudo tee -a /etc/security/limits.conf > /dev/null <<EOT

# >> VPN CONFIGURATION
@${GROUP}   hard    maxlogins   1
# << VPN CONFIGURATION
EOT

sudo tee -a /etc/ssh/sshd_config > /dev/null <<EOT

# >> VPN CONFIGURATION
AllowGroups root ${CURRENT_GROUP} ${GROUP}
AddressFamily inet
TCPKeepAlive no
ClientAliveInterval 30
ClientAliveCountMax 240
Match Group                     ${GROUP}
    PubkeyAuthentication        yes
    PasswordAuthentication      yes
    PermitEmptyPasswords        no
    GatewayPorts                no
    ForceCommand                internal-sftp
    AllowTcpForwarding          yes
    HostbasedAuthentication     no
    RhostsRSAAuthentication     no
    AllowAgentForwarding        no
    Banner                      none
# << VPN CONFIGURATION
EOT

sudo tee -a /etc/sysctl.conf > /dev/null <<EOT

# >> VPN CONFIGURATION
net.ipv6.conf.all.disable_ipv6 = 1
net.ipv6.conf.default.disable_ipv6 = 1
# << VPN CONFIGURATION
EOT

sudo sysctl -p

sudo service ssh reload
