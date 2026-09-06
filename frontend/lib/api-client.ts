const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

/**
 * Wrapper único para chamadas à API. Centralizar aqui evita repetir a lógica
 * de headers/erro em cada página — e é o único lugar que vai precisar mudar
 * na FASE 3 para incluir o header "Authorization: Bearer <token>".
 */
export async function apiFetch<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers,
      // Authorization header entra aqui na FASE 3, lendo o token do storage
      // de sessão escolhido para o app.
    },
  });

  if (!response.ok) {
    const message = await response.text().catch(() => response.statusText);
    throw new Error(`Erro ${response.status}: ${message}`);
  }

  return response.json() as Promise<T>;
}
