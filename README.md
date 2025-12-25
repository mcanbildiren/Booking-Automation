docker run -d --name booking-ngrok -p 4040:4040 -e NGROK_AUTHTOKEN=$NGROK_AUTHTOKEN ngrok/ngrok:latest http host.docker.internal:5001

docker ngrok command