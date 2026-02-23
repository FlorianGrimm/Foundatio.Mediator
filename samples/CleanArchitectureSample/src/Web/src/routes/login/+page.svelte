<script lang="ts">
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  import { auth } from '$lib/stores/auth.svelte';

  let username = $state('');
  let password = $state('');

  // Redirect to the page they were trying to visit, or home
  let redirectTo = $derived($page.url.searchParams.get('redirect') || '/');

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();
    await auth.login({ username, password });
    if (auth.isAuthenticated) {
      goto(redirectTo);
    }
  }

  // If already authenticated, redirect immediately
  $effect(() => {
    if (!auth.loading && auth.isAuthenticated) {
      goto(redirectTo);
    }
  });
</script>

<svelte:head>
  <title>Sign In - Clean Architecture Sample</title>
</svelte:head>

<div class="flex items-center justify-center py-12">
  <div class="w-full max-w-sm bg-white rounded-lg shadow-md p-8">
    <h1 class="text-2xl font-bold text-gray-900 text-center mb-2">Sign In</h1>
    <p class="text-sm text-gray-500 text-center mb-6">
      Sign in to access orders, reports, and product management.
    </p>

    {#if auth.error}
      <div class="mb-4 rounded-md bg-red-50 p-3 text-sm text-red-700">
        {auth.error}
      </div>
    {/if}

    <form onsubmit={handleSubmit}>
      <label class="block mb-4">
        <span class="block text-sm font-medium text-gray-700 mb-1">Username</span>
        <input
          type="text"
          bind:value={username}
          class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:ring-blue-500"
          placeholder="admin or user"
          required
        />
      </label>

      <label class="block mb-6">
        <span class="block text-sm font-medium text-gray-700 mb-1">Password</span>
        <input
          type="password"
          bind:value={password}
          class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:ring-blue-500"
          placeholder="same as username"
          required
        />
      </label>

      <button
        type="submit"
        disabled={auth.loading}
        class="w-full rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 transition-colors disabled:opacity-50"
      >
        {auth.loading ? 'Signing in…' : 'Sign in'}
      </button>
    </form>

    <div class="mt-6 rounded-md bg-gray-50 p-3 text-xs text-gray-500">
      <p class="font-medium mb-1">Demo accounts:</p>
      <p><strong>admin</strong> / admin &mdash; Alice Admin (Admin)</p>
      <p><strong>user</strong> / user &mdash; Bob User (User)</p>
    </div>
  </div>
</div>
