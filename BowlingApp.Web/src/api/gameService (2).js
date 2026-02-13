const API_URL = import.meta.env.VITE_API_URL;

export async function createGame(players) {
  const response = await fetch(API_URL, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(players),
  });
  return await response.json();
}

export async function getGame(id) {
  const response = await fetch(`${API_URL}/${id}`);
  return await response.json();
}

export async function rollBall(gameId, playerId, pins) {
  await fetch(`${API_URL}/${gameId}/roll`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ playerId, pins }),
  });
}
