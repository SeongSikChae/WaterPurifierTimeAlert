import type { User } from '@/types/filterType';

export const userClient = {
  whoami: async (): Promise<User> => {
    const response = await fetch('/api/User/whoami', { credentials: 'include' });
    if (!response.ok) throw new Error(`${response.status} ${response.statusText}`);
    return (await response.json()) as User;
  },
};
