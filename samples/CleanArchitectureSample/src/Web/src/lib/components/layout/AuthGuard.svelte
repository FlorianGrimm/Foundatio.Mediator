<script lang="ts">
  import { page } from '$app/stores';
  import { auth } from '$lib/stores/auth.svelte';
  import { Button } from '$lib/components/ui';
  import type { Snippet } from 'svelte';

  let { children }: { children: Snippet } = $props();

  let loginUrl = $derived(`/login?redirect=${encodeURIComponent($page.url.pathname)}`);
</script>

{#if auth.loading}
  <div class="flex justify-center py-12">
    <p class="text-gray-500">Checking authentication…</p>
  </div>
{:else if !auth.isAuthenticated}
  <div class="flex flex-col items-center justify-center py-16 text-center">
    <div class="rounded-full bg-gray-100 p-4 mb-4">
      <svg class="h-8 w-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
      </svg>
    </div>
    <h2 class="text-lg font-semibold text-gray-900 mb-1">Authentication Required</h2>
    <p class="text-sm text-gray-500 mb-6">You need to sign in to access this page.</p>
    <Button href={loginUrl}>Sign in</Button>
  </div>
{:else}
  {@render children()}
{/if}
