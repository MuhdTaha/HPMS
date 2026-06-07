const env = (globalThis as { process?: { env?: Record<string, string> } }).process?.env;

export const API_BASE_URL = env?.['apiBaseUrl'] ?? 'http://localhost:5260';
export const IDENTITY_API_URL = env?.['apiUrl'] ?? `${API_BASE_URL}/identity`;
