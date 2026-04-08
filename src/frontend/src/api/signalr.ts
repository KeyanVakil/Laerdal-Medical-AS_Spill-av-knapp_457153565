import * as signalR from '@microsoft/signalr';

function createConnection(hubPath: string): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(hubPath)
    .withAutomaticReconnect()
    .build();
}

export const cprConnection = createConnection('/hubs/cpr');
export const monitorConnection = createConnection('/hubs/monitor');

export async function startConnection(connection: signalR.HubConnection): Promise<void> {
  if (connection.state === signalR.HubConnectionState.Disconnected) {
    await connection.start();
  }
}

export async function stopConnection(connection: signalR.HubConnection): Promise<void> {
  if (connection.state === signalR.HubConnectionState.Connected) {
    await connection.stop();
  }
}
