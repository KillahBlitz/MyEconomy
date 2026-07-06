<script setup>
import { ref } from "vue";
import { getCategories, createCategory, deleteCategory } from "../../services/api.js";

const toast = ref({ visible: false, message: "", type: "error" });
let toastTimer = null;

const showToast = (message, type = "error") => {
    if (toastTimer) clearTimeout(toastTimer);
    toast.value = { visible: true, message, type };
    toastTimer = setTimeout(() => { toast.value.visible = false; }, 3000);
};

const showCreateModal = ref(false);
const newCategoryName = ref("");
const isCreating = ref(false);

const showDeleteModal = ref(false);
const categoriesList = ref([]);
const selectedCategoryId = ref(null);
const isDeleting = ref(false);
const isLoadingCategories = ref(false);

const openCreateModal = () => {
    newCategoryName.value = "";
    showCreateModal.value = true;
};

const closeCreateModal = () => {
    showCreateModal.value = false;
};

const handleCreate = async () => {
    if (!newCategoryName.value.trim()) {
        showToast("El nombre es obligatorio.");
        return;
    }
    if (newCategoryName.value.trim().length > 10) {
        showToast("Máximo 10 caracteres.");
        return;
    }
    isCreating.value = true;
    try {
        const result = await createCategory(newCategoryName.value.trim());
        if (result === true) {
            showToast("Categoría creada correctamente", "success");
            closeCreateModal();
        } else {
            showToast("No se pudo crear la categoría.");
        }
    } catch {
        showToast("Error al crear la categoría.");
    } finally {
        isCreating.value = false;
    }
};

const openDeleteModal = async () => {
    selectedCategoryId.value = null;
    isLoadingCategories.value = true;
    showDeleteModal.value = true;
    try {
        const data = await getCategories();
        categoriesList.value = Array.isArray(data) ? data : [];
    } catch {
        categoriesList.value = [];
        showToast("Error al cargar categorías.");
    } finally {
        isLoadingCategories.value = false;
    }
};

const closeDeleteModal = () => {
    showDeleteModal.value = false;
};

const handleDelete = async () => {
    if (selectedCategoryId.value == null) {
        showToast("Selecciona una categoría.");
        return;
    }
    isDeleting.value = true;
    try {
        const result = await deleteCategory(selectedCategoryId.value);
        if (result === true) {
            showToast("Categoría eliminada correctamente", "success");
            closeDeleteModal();
        } else {
            showToast("No se pudo eliminar la categoría.");
        }
    } catch {
        showToast("Error al eliminar la categoría.");
    } finally {
        isDeleting.value = false;
    }
};
</script>

<template>
    <div class="config-screen">
        <transition name="toast-fade">
            <div v-if="toast.visible" :class="['toast', `toast-${toast.type}`]" role="status" aria-live="polite">
                {{ toast.message }}
            </div>
        </transition>

        <transition name="modal-fade">
            <div v-if="showCreateModal" class="modal-overlay" @click.self="closeCreateModal">
                <div class="modal-content">
                    <header class="modal-header">
                        <h2>Crear Categoría</h2>
                        <button class="modal-close" type="button" @click="closeCreateModal">&times;</button>
                    </header>
                    <div class="modal-body">
                        <div class="modal-field">
                            <label for="cat-name">Nombre (máx. 10 caracteres)</label>
                            <input
                                id="cat-name"
                                v-model="newCategoryName"
                                type="text"
                                maxlength="10"
                                placeholder="Ej: Comida"
                                class="modal-input"
                            />
                        </div>
                    </div>
                    <footer class="modal-footer">
                        <button class="modal-cancel-btn" type="button" @click="closeCreateModal">Cancelar</button>
                        <button class="modal-submit-btn" type="button" @click="handleCreate" :disabled="isCreating">
                            <span v-if="!isCreating">Crear</span>
                            <span v-else class="button-spinner"></span>
                        </button>
                    </footer>
                </div>
            </div>
        </transition>

        <transition name="modal-fade">
            <div v-if="showDeleteModal" class="modal-overlay" @click.self="closeDeleteModal">
                <div class="modal-content modal-content-danger">
                    <header class="modal-header modal-header-danger">
                        <h2>Eliminar Categoría</h2>
                        <button class="modal-close" type="button" @click="closeDeleteModal">&times;</button>
                    </header>
                    <div class="modal-body">
                        <p class="modal-question">¿Qué categoría desea eliminar?</p>
                        <div v-if="isLoadingCategories" class="loading-text">Cargando...</div>
                        <div v-else class="category-radio-list">
                            <label
                                v-for="cat in categoriesList"
                                :key="cat.categoria_id"
                                class="radio-item"
                                :class="{ active: selectedCategoryId === cat.categoria_id }"
                            >
                                <input
                                    type="radio"
                                    :value="cat.categoria_id"
                                    v-model="selectedCategoryId"
                                />
                                <span>{{ cat.tipo_categoria }}</span>
                            </label>
                        </div>
                    </div>
                    <footer class="modal-footer">
                        <button class="modal-cancel-btn" type="button" @click="closeDeleteModal">Cancelar</button>
                        <button class="modal-submit-btn modal-submit-danger" type="button" @click="handleDelete" :disabled="isDeleting || selectedCategoryId == null">
                            <span v-if="!isDeleting">Confirmar</span>
                            <span v-else class="button-spinner"></span>
                        </button>
                    </footer>
                </div>
            </div>
        </transition>

        <section class="config-card">
            <div class="card-icon">&#9776;</div>
            <div class="card-info">
                <h2>Configurar Categorías</h2>
                <p class="card-description">Administra las categorías de gastos disponibles en tu contabilidad. Puedes crear nuevas o eliminar las que ya no necesites.</p>
            </div>
            <div class="card-actions">
                <button class="btn-create" type="button" @click="openCreateModal">Crear categoría</button>
                <button class="btn-delete" type="button" @click="openDeleteModal">Eliminar categoría</button>
            </div>
        </section>
    </div>
</template>

<style scoped>
.config-screen {
    width: 100%;
    min-height: calc(100vh - 60px);
    padding: 24px 16px;
    position: relative;
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.config-card {
    background: rgba(15, 23, 42, 0.95);
    border: 1px solid rgba(44, 199, 124, 0.35);
    border-radius: 16px;
    padding: 24px 28px;
    color: #ffffff;
    display: flex;
    align-items: center;
    gap: 20px;
    box-shadow: 0 10px 24px rgba(15, 23, 42, 0.22);
    transition: border-color 0.2s ease;
}

.config-card:hover {
    border-color: rgba(44, 199, 124, 0.6);
}

.card-icon {
    font-size: 28px;
    width: 52px;
    height: 52px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(44, 199, 124, 0.1);
    border: 1px solid rgba(44, 199, 124, 0.3);
    border-radius: 12px;
    flex-shrink: 0;
}

.card-info {
    flex: 1;
    min-width: 0;
}

.config-card h2 {
    margin: 0 0 6px;
    font-size: 16px;
    font-weight: 700;
    color: var(--primary-color);
}

.card-description {
    margin: 0;
    font-size: 13px;
    color: rgba(255, 255, 255, 0.65);
    line-height: 1.4;
}

.card-actions {
    display: flex;
    flex-direction: column;
    gap: 8px;
    flex-shrink: 0;
}

.btn-create {
    padding: 9px 20px;
    border: none;
    border-radius: 8px;
    background: linear-gradient(135deg, #4dabf7, #228be6);
    color: #ffffff;
    font-weight: 700;
    font-size: 12px;
    cursor: pointer;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
    white-space: nowrap;
}

.btn-create:hover {
    transform: translateY(-1px);
    box-shadow: 0 4px 10px rgba(77, 171, 247, 0.3);
}

.btn-delete {
    padding: 9px 20px;
    border: none;
    border-radius: 8px;
    background: linear-gradient(135deg, #ff6b6b, #e03131);
    color: #ffffff;
    font-weight: 700;
    font-size: 12px;
    cursor: pointer;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
    white-space: nowrap;
}

.btn-delete:hover {
    transform: translateY(-1px);
    box-shadow: 0 4px 10px rgba(255, 107, 107, 0.3);
}

.toast {
    position: fixed;
    top: 24px;
    right: 24px;
    z-index: 100;
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

.modal-overlay {
    position: fixed;
    inset: 0;
    z-index: 200;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(0, 0, 0, 0.6);
    backdrop-filter: blur(4px);
}

.modal-content {
    background: rgba(15, 23, 42, 0.98);
    border: 1px solid rgba(77, 171, 247, 0.5);
    border-radius: 16px;
    padding: 24px;
    width: min(90%, 360px);
    box-shadow: 0 20px 50px rgba(0, 0, 0, 0.4);
    color: #ffffff;
}

.modal-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
    padding-bottom: 10px;
    border-bottom: 1px solid rgba(77, 171, 247, 0.3);
}

.modal-header h2 {
    margin: 0;
    font-size: 16px;
    font-weight: 700;
    color: #4dabf7;
}

.modal-close {
    background: none;
    border: none;
    color: rgba(255, 255, 255, 0.6);
    font-size: 22px;
    cursor: pointer;
    padding: 0 4px;
    line-height: 1;
    transition: color 0.15s ease;
}

.modal-close:hover {
    color: #ffffff;
}

.modal-body {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.modal-field {
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.modal-field label {
    font-size: 12px;
    font-weight: 600;
    color: rgba(255, 255, 255, 0.85);
}

.modal-input {
    padding: 9px 12px;
    border-radius: 8px;
    border: 1px solid rgba(77, 171, 247, 0.4);
    background: rgba(0, 0, 0, 0.4);
    color: #ffffff;
    font-size: 13px;
    outline: none;
    transition: border-color 0.2s ease;
}

.modal-input:focus {
    border-color: #4dabf7;
}

.modal-input::placeholder {
    color: rgba(255, 255, 255, 0.4);
}

.modal-footer {
    display: flex;
    justify-content: flex-end;
    gap: 10px;
    margin-top: 16px;
}

.modal-cancel-btn {
    padding: 7px 14px;
    border: 1px solid rgba(255, 255, 255, 0.2);
    border-radius: 8px;
    background: transparent;
    color: rgba(255, 255, 255, 0.8);
    font-size: 12px;
    font-weight: 600;
    cursor: pointer;
    transition: background 0.15s ease;
}

.modal-cancel-btn:hover {
    background: rgba(255, 255, 255, 0.08);
}

.modal-submit-btn {
    padding: 7px 16px;
    border: none;
    border-radius: 8px;
    background: linear-gradient(135deg, #4dabf7, #228be6);
    color: #ffffff;
    font-size: 12px;
    font-weight: 700;
    cursor: pointer;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
    display: flex;
    align-items: center;
    justify-content: center;
    min-width: 80px;
}

.modal-submit-btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

.modal-submit-btn:hover:not(:disabled) {
    transform: translateY(-1px);
    box-shadow: 0 4px 10px rgba(77, 171, 247, 0.3);
}

.modal-content-danger {
    border-color: rgba(255, 107, 107, 0.5);
}

.modal-header-danger {
    border-bottom-color: rgba(255, 107, 107, 0.3);
}

.modal-header-danger h2 {
    color: #ff6b6b;
}

.modal-submit-danger {
    background: linear-gradient(135deg, #ff6b6b, #e03131);
}

.modal-submit-danger:hover:not(:disabled) {
    box-shadow: 0 4px 10px rgba(255, 107, 107, 0.3);
}

.modal-question {
    margin: 0;
    font-size: 13px;
    color: rgba(255, 255, 255, 0.8);
}

.loading-text {
    font-size: 12px;
    color: rgba(255, 255, 255, 0.5);
    text-align: center;
    padding: 12px 0;
}

.category-radio-list {
    display: flex;
    flex-direction: column;
    gap: 4px;
    max-height: 180px;
    overflow-y: auto;
}

.radio-item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 7px 10px;
    border-radius: 6px;
    cursor: pointer;
    font-size: 12px;
    color: #ffffff;
    transition: background 0.15s ease;
}

.radio-item:hover {
    background: rgba(255, 255, 255, 0.06);
}

.radio-item.active {
    background: rgba(255, 107, 107, 0.15);
}

.radio-item input {
    accent-color: #ff6b6b;
    width: 14px;
    height: 14px;
    cursor: pointer;
}

.button-spinner {
    width: 14px;
    height: 14px;
    border-radius: 50%;
    border: 2px solid rgba(255, 255, 255, 0.3);
    border-top-color: #ffffff;
    animation: spin 0.8s linear infinite;
    display: inline-block;
}

@keyframes spin {
    to { transform: rotate(360deg); }
}

.modal-fade-enter-active,
.modal-fade-leave-active {
    transition: opacity 0.2s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
    opacity: 0;
}
</style>
