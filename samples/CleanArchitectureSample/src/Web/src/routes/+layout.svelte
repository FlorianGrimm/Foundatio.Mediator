<script lang="ts">
  import '../app.css';
  import { Header, Sidebar, ToastContainer } from '$lib/components/layout';
  import { signalr } from '$lib/stores/signalr.svelte';
  import { auth } from '$lib/stores/auth.svelte';
  import { onMount } from 'svelte';

  let { children } = $props();

  onMount(() => {
    auth.check();
    signalr.start();
    return () => signalr.stop();
  });
</script>

{#if auth.loading}
  <div class="min-h-screen bg-gray-50 flex items-center justify-center">
    <p class="text-gray-500">Loading…</p>
  </div>
{:else}
  <div class="min-h-screen bg-gray-50">
    <Header />
    <div class="flex">
      <Sidebar />
      <main class="flex-1 p-6">
        {@render children()}
      </main>
    </div>
    <ToastContainer />
  </div>
{/if}
