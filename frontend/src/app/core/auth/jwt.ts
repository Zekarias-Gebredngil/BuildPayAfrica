export type JwtPayload = {
  sub?: string;
  preferred_username?: string;
  email?: string;
  name?: string;
  realm_access?: { roles?: string[] };
  resource_access?: Record<string, { roles?: string[] }>;
  exp?: number;
};

export function decodeJwt(token: string): JwtPayload | null {
  const parts = token.split('.');
  if (parts.length !== 3) return null;

  try {
    const payload = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const padded = payload + '='.repeat((4 - (payload.length % 4)) % 4);
    const json = atob(padded);
    return JSON.parse(json) as JwtPayload;
  } catch {
    return null;
  }
}

export function getJwtRoles(payload: JwtPayload | null): string[] {
  if (!payload) return [];
  const realmRoles = payload.realm_access?.roles ?? [];
  const clientRoles = Object.values(payload.resource_access ?? {})
    .flatMap((x) => x.roles ?? []);
  return Array.from(new Set([...realmRoles, ...clientRoles]));
}

