<script setup>
import { useRouter } from "vue-router";
import Header from "./templates/Header.vue"

const router = useRouter();

const handleLogout = async () => {
    localStorage.removeItem("userId");
    await router.replace({ name: "login" });
};
</script>

<template>
    <div class="principal-page">
        <Header @logout="handleLogout" />
        <main class="principal-content">
            <router-view />
        </main>
    </div>
</template>

<style scoped>
.principal-page {
    min-height: 100vh;
    width: 100%;
    display: flex;
    flex-direction: column;
}

.principal-content {
    flex: 1;
    width: 100%;
}
</style>