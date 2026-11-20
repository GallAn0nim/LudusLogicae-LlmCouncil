#!/bin/zsh
# Double-click in Finder to run the API (http://localhost:5160) and frontend (http://localhost:3000).
# Close this window or press Ctrl+C to stop both.

cd "${0:A:h}" || exit 1
export PATH="/opt/homebrew/bin:/usr/local/share/dotnet:/usr/local/bin:$PATH"

API_PORT=5160
WEB_PORT=3000

fail() { print -P "%F{red}$1%f"; read -k1 "?Press any key to close..."; exit 1; }
port_busy() { lsof -nP -iTCP:$1 -sTCP:LISTEN >/dev/null 2>&1; }

command -v dotnet >/dev/null || fail "dotnet not found. Install the .NET 11 SDK."
command -v npm >/dev/null || fail "npm not found. Install Node.js."

if ! pg_isready -q 2>/dev/null; then
  echo "Starting PostgreSQL..."
  brew services start postgresql@18 >/dev/null
  for i in {1..20}; do pg_isready -q && break; sleep 0.5; done
  pg_isready -q || fail "PostgreSQL is not running."
fi

port_busy $API_PORT && fail "Port $API_PORT is already in use. Is the API already running?"
port_busy $WEB_PORT && fail "Port $WEB_PORT is already in use. Is the frontend already running?"

[[ -d frontend/node_modules ]] || { echo "Installing frontend packages..."; (cd frontend && npm ci --no-audit --no-fund) || fail "npm install failed."; }

stop() {
  trap - INT TERM HUP
  echo "\nStopping..."
  kill $(lsof -tiTCP:$API_PORT -sTCP:LISTEN) $(lsof -tiTCP:$WEB_PORT -sTCP:LISTEN) 2>/dev/null
  pkill -P $$ 2>/dev/null
  wait 2>/dev/null
  exit 0
}
trap stop INT TERM HUP

echo "Starting API on http://localhost:$API_PORT ..."
dotnet run --project src/LudusLogicae.Api --launch-profile http 2>&1 | sed -l 's/^/[api] /' &

echo "Starting frontend on http://localhost:$WEB_PORT ..."
(cd frontend && npx vite --port $WEB_PORT --strictPort) 2>&1 | sed -l 's/^/[web] /' &

for i in {1..120}; do port_busy $API_PORT && port_busy $WEB_PORT && break; sleep 0.5; done
if port_busy $API_PORT && port_busy $WEB_PORT; then
  [[ -n $NO_BROWSER ]] || open "http://localhost:$WEB_PORT"
  print -P "%F{green}Running. Frontend: http://localhost:$WEB_PORT  Swagger: http://localhost:$API_PORT/swagger%f"
  print -P "%F{green}Close this window or press Ctrl+C to stop.%f"
else
  print -P "%F{yellow}Still starting. Check the log lines above.%f"
fi

wait
