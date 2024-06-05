echo "stopping running service ... "
sudo systemctl stop worker2 # stop the worker2 service to remove any file-locks
echo "service stopped."
echo "Republishing service..."
sudo dotnet publish -c Release -o /srv/worker2 # release to your user directory
sudo cp .env /srv/worker2/.env
sudo cp *.json /srv/worker2/


echo "Updating systemctl ..."
sudo cp worker2.service /etc/systemd/system/worker2.service
sudo systemctl daemon-reload
sudo systemctl start worker2  

echo "restarting service..."
sudo systemctl start worker2 # start service
