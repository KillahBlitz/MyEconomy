<script setup>
import { useRouter } from 'vue-router';

const emit = defineEmits(["logout"]);

const router = useRouter();

const goToContability = () => router.push({ name: 'principal-contability' });
const goToDebts = () => router.push({ name: 'principal-debts' });
const goToTickets = () => router.push({ name: 'principal-tickets' });
</script>

<template>
    <div class="header-box">
        <div class="header-title">
            <div class="header-left-group">
                <img alt="Alien Gains Icon" class="logo" src="../../assets/icons/favicon.svg"/>
                <button class="header-button-1" @click="goToContability">Contabilidad</button>
                <button class="header-button-1" @click="goToDebts">Deudas</button>
                <button class="header-button-1" @click="goToTickets">Tickets</button>
                <button class="header-button-2">Configuracion</button>
            </div>
            <button class="header-button-3" @click="emit('logout')">Cerrar Sesion</button>
        </div>
    </div>
</template>

<style scoped>
.header-box {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 10px;
    height: 80px;
    width: 100%;
    backdrop-filter: blur(14px);
    -webkit-backdrop-filter: blur(14px);
}

.header-box img {
    width: 50px;
    height: 50px;
    background-color: #2cc77c;
    border-radius: 50%;
}

.header-title {
    display: flex;
    align-items: center;
    width: 100%;
    justify-content: space-between;
    gap: 14px;
}

.header-left-group {
    display: flex;
    align-items: center;
    gap: 14px;
    min-width: 0;
    flex: 1;
}

.header-title button {
    font-size: 14px;
    border: 1px solid transparent;
    box-sizing: border-box;
    padding: 8px 16px;
    border-radius: 4px;
    cursor: pointer;
    transition: transform 0.15s ease, background-color 0.15s ease, color 0.15s ease, border-color 0.15s ease;
    will-change: transform;
}

.header-title button:hover {
    transform: scale(1.06);
    background-color: transparent;
}

.header-title button:active {
    transform: scale(0.94);
}

.header-button-1 {
    --button-bg: var(--primary-color);
    background-color: var(--button-bg);
    color: #0f172a;
}

.header-button-1:hover {
    color: var(--button-bg);
    border: 1px solid var(--button-bg);
}

.header-button-1:active {
    --button-bg: var(--primary-color);
}

.header-button-2 {
    --button-bg: var(--secondary-color);
    background-color: var(--button-bg);
    color: #0f172a;
}

.header-button-2:hover {
    color: var(--button-bg);
    border: 1px solid var(--button-bg);
}

.header-button-2:active {
    --button-bg: var(--secondary-color);
}

.header-button-3 {
    --button-bg: var(--alert-color);
    background-color: var(--button-bg);
    color: var(--vt-c-white);
    flex-shrink: 0;
}

.header-button-3:hover {
    color: var(--button-bg);
    border: 1px solid var(--button-bg);
}

.header-button-1,
.header-button-2,
.header-button-3 {
    margin-right: 0;
    margin-left: 0;
}
</style>