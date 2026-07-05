<template>
    <div class="login-container">
        <transition name="toast-fade">
            <div v-if="toast.visible" :class="['toast', `toast-${toast.type}`]" role="status" aria-live="polite">
                {{ toast.message }}
            </div>
        </transition>
        <div class="login-box">
            <div class="login-title">
                <h2>Iniciar Sesion</h2>
            </div>
            <div class="login-form">
                <form class="login-form-element" @submit.prevent="handleSubmit">
                    <div class="form-group">
                        <label for="username">Ingresar Usuario</label>
                        <input type="text" id="username" name="username" placeholder="Tu usuario" required />

                        <label for="password">Ingresa Contraseña</label>
                        <input type="password" id="password" name="password" placeholder="Tu contraseña" required />

                        <button type="submit" :disabled="awaitLogin">
                            <span v-if="!awaitLogin">Iniciar Sesion</span>
                            <span v-else class="button-spinner" aria-hidden="true"></span>
                        </button>
                    </div>
                </form>
            </div>
        </div>  
    </div>
</template>

<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import { loginUser } from "../services/api.js";

const awaitLogin = ref(false);
const toast = ref({
    visible: false,
    message: "",
    type: "error",
});
let toastTimer = null;

const showToast = (message, type = "error") => {
    if (toastTimer) {
        clearTimeout(toastTimer);
    }

    toast.value = {
        visible: true,
        message,
        type,
    };

    toastTimer = setTimeout(() => {
        toast.value.visible = false;
    }, 3000);
};

const router = useRouter();

const handleSubmit = async (event) => {
    const form = event.target;
    const username = form.username.value;
    const password = form.password.value;
    if (!username || !password) {
        showToast("Por favor, ingresa tu usuario y contraseña.");
        return;
    }
    else if (username.length > 10) {
        showToast("Usuario inválido.");
        return;
    }
    else if (password.length > 20) {
        showToast("Contraseña inválida.");
        return;
    }
    awaitLogin.value = true;
    try {
        const response = await loginUser(username, password);
        console.log("Login response:", response);
        if (response.success) {
            localStorage.setItem("userId", response.userId);
            showToast("Inicio de sesión exitoso.", "success");
            await router.replace({ name: "principal" });
        } else {
            showToast(response.error || "No se pudo iniciar sesión.");
        }
    } catch (error) {
        showToast(error?.message || "Error inesperado al iniciar sesión.");
    } finally {
        awaitLogin.value = false;
    }
};

</script>

<style scoped>
.login-container {
    display: flex;
    align-items: center;
    justify-content: center;
    height: 100vh;
    width: 100%;
    position: relative;
}

.toast {
    position: fixed;
    top: 24px;
    right: 24px;
    z-index: 50;
    min-width: 280px;
    max-width: 360px;
    padding: 14px 16px;
    border-radius: 12px;
    box-shadow: 0 14px 30px rgba(15, 23, 42, 0.18);
    color: #0f172a;
    font-size: 14px;
    font-weight: 600;
}

.toast-error {
    background: #ff0000;
    border: 1px solid #c72828;
    color: #fff7f7;
}

.toast-success {
    background: #00ff59;
    border: 1px solid #86efac;
}

.toast-fade-enter-active,
.toast-fade-leave-active {
    transition: opacity 0.2s ease, transform 0.2s ease;
}

.toast-fade-enter-from,
.toast-fade-leave-to {
    opacity: 0;
    transform: translateY(-8px);
}

.login-box {
    width: 100%;
    max-width: 420px;
    padding: 32px;
    border-radius: 16px;
    box-shadow: 0 18px 50px rgba(44, 199, 124, 0.18);
    border: 1px solid rgba(44, 199, 124, 0.15);
    transition: box-shadow 0.3s ease;
}

.login-title {
    text-align: center;
    margin-bottom: 24px;
    color: var(--primary-color);
}

.login-title h2 {
    font-size: 28px;
    font-weight: 700;
}

.login-form {
    width: 100%;
}

.login-form-element {
    width: 100%;
}

.form-group {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.form-group label {
    font-size: 14px;
    font-weight: 600;
}

.form-group input {
    width: 100%;
    padding: 12px 14px;
    border: 1px solid #cbd5e1;
    border-radius: 10px;
    font-size: 15px;
    outline: none;
    transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.form-group input:focus {
    border-color: var(--secondary-color);
    box-shadow: 0 0 0 3px rgba(44, 199, 124, 0.16);
}

.form-group button {
    margin-top: 8px;
    width: 100%;
    padding: 12px 16px;
    border: none;
    border-radius: 10px;
    background: linear-gradient(135deg, var(--secondary-color), var(--primary-color));
    color: #0f172a;
    font-size: 15px;
    font-weight: 700;
    cursor: pointer;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
}

.form-group button:disabled {
    cursor: not-allowed;
    opacity: 0.75;
    transform: none;
    box-shadow: none;
}

.form-group button:hover {
    transform: translateY(-1px);
    box-shadow: 0 10px 18px rgba(44, 199, 124, 0.22);
}

.button-spinner {
    width: 16px;
    height: 16px;
    border-radius: 50%;
    border: 2px solid rgba(44, 199, 124, 0.2);
    border-top-color: var(--secondary-color);
    animation: spin 0.8s linear infinite;
    display: inline-block;
}

@keyframes spin {
    to {
        transform: rotate(360deg);
    }
}

.login-box:hover {
    box-shadow: 0 18px 50px rgba(44, 199, 124, 0.28);
    transition: box-shadow 0.3s ease;
}
</style>