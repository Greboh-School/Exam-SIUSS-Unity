# Use an appropriate base image (e.g., Ubuntu)
FROM ubuntu:20.04

# Install necessary dependencies
# RUN apt-get update && apt-get install -y \
    # Add any specific dependencies your Unity server needs here
    # For example: libsdl2-2.0-0 libglu1-mesa

# Copy your Unity server files
COPY . /app

# Set the working directory
WORKDIR /app

# Command to run your Unity server
CMD ["sh", "-c", "./LinuxServer.x86_64 $UNITY_SERVER_IP"]