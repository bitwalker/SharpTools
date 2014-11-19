# Set output level to verbose and make the script stop on error
$VerbosePreference     = "Continue" # Use "SilentlyContinue" if you don't want the verbosity
$ErrorActionPreference = "Stop"

# Opens a socket to a TCP server, and sends a plain text command
# Example: . SendCommand "127.0.0.1" 3000 "cmd"
function SendCommand ($ipAddress, $port, $command) {

  trap { write-error "Failed to connect to ${ipAddress}:$port - $_"; exit }

  $socket = new-object System.Net.Sockets.TcpClient($ipAddress, $port)
  $stream = $socket.GetStream()
  $commandWriter = new-object System.IO.StreamWriter($stream)
  $commandWriter.Write($command)
  $commandWriter.Flush()
  $commandWriter.Close()
  $stream.Close()
}