<script setup>
import { computed, ref } from "vue";
import { VueDatePicker } from "@vuepic/vue-datepicker";
import "@vuepic/vue-datepicker/dist/main.css";
import { getDashboardDebs, createDeuda, deleteDeuda } from "../../services/api.js";

const dateRange = ref([null, null]);
const isLoading = ref(false);
const dashboardData = ref(null);

const showDebtModal = ref(false);
const debtForm = ref({ descripcion: "", cantidad: 0 });
const isCreatingDebt = ref(false);

const showCoverModal = ref(false);
const coverForm = ref({ descripcion: "", cantidad: 0 });
const isCreatingCover = ref(false);

const toast = ref({ visible: false, message: "", type: "error" });
let toastTimer = null;

const showToast = (message, type = "error") => {
    if (toastTimer) clearTimeout(toastTimer);
    toast.value = { visible: true, message, type };
    toastTimer = setTimeout(() => { toast.value.visible = false; }, 3000);
};

const startDate = computed(() => dateRange.value?.[0] ?? null);
const endDate = computed(() => dateRange.value?.[1] ?? null);

const currencyFormatter = new Intl.NumberFormat("es-MX", {
    style: "currency",
    currency: "MXN",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
});
const formatCurrency = (amount) => currencyFormatter.format(Number(amount) || 0);

const donutRadius = 60;
const donutCircumference = computed(() => 2 * Math.PI * donutRadius);
const donutPercent = computed(() => {
    const raw = dashboardData.value?.porcentajeGasto ?? 0;
    return Math.max(0, Math.min(100, raw));
});
const donutDashOffset = computed(() => {
    return donutCircumference.value * (1 - donutPercent.value / 100);
});

const saldoPendiente = computed(() => {
    if (!dashboardData.value) return 0;
    const diff = (dashboardData.value.totalSaldoDeuda ?? 0) - (dashboardData.value.totalSaldoCubierto ?? 0);
    return Math.max(0, diff);
});

const formatDate = (date) => {
    if (!date) return null;
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
};

const fetchDashboard = async () => {
    if (!dateRange.value || !dateRange.value[0] || !dateRange.value[1]) {
        showToast("Por favor selecciona un rango de fechas.");
        return;
    }

    isLoading.value = true;

    try {
        const params = {
            startDate: formatDate(dateRange.value[0]),
            endDate: formatDate(dateRange.value[1])
        };

        const data = await getDashboardDebs(params);

        if (data && data.success === false) {
            dashboardData.value = null;
            showToast(data.error || "No se pudo obtener el dashboard.");
            return;
        }

        dashboardData.value = data;
    } catch (error) {
        dashboardData.value = null;
        showToast("Ocurrió un error al obtener el dashboard.");
    } finally {
        isLoading.value = false;
    }
};

const handleDeleteDeuda = async (deudaId) => {
    try {
        const result = await deleteDeuda(deudaId);
        if (result === true) {
            if (dashboardData.value) {
                dashboardData.value.deudas = dashboardData.value.deudas?.filter(d => d.deuda_id !== deudaId) ?? [];
                dashboardData.value.cubierto = dashboardData.value.cubierto?.filter(d => d.deuda_id !== deudaId) ?? [];
            }
            showToast("Registro eliminado correctamente", "success");
        } else {
            showToast("No se pudo eliminar el registro.");
        }
    } catch (error) {
        showToast("Ocurrió un error al eliminar el registro.");
    }
};

const openDebtModal = () => {
    debtForm.value = { descripcion: "", cantidad: 0 };
    showDebtModal.value = true;
};

const closeDebtModal = () => {
    showDebtModal.value = false;
};

const handleCreateDebt = async () => {
    if (!debtForm.value.descripcion.trim()) {
        showToast("La descripción es obligatoria.");
        return;
    }
    if (debtForm.value.cantidad <= 0) {
        showToast("La cantidad debe ser mayor a 0.");
        return;
    }

    const cachedUserId = localStorage.getItem('userId');
    const userId = cachedUserId ? parseInt(cachedUserId, 10) : 0;
    if (userId <= 0) { showToast("No se encontró el usuario."); return; }

    isCreatingDebt.value = true;
    try {
        const result = await createDeuda({
            id_usuario: userId,
            tipo_deuda: true,
            descripcion: debtForm.value.descripcion.trim(),
            cantidad: Number(debtForm.value.cantidad),
            fcha_registro: formatDate(new Date())
        });
        if (result === true) {
            showToast("Registrado correctamente", "success");
            closeDebtModal();
            dashboardData.value = null;
        } else {
            showToast("No se pudo registrar la deuda.");
        }
    } catch (error) {
        showToast("Ocurrió un error al registrar la deuda.");
    } finally {
        isCreatingDebt.value = false;
    }
};

const openCoverModal = () => {
    coverForm.value = { descripcion: "", cantidad: 0 };
    showCoverModal.value = true;
};

const closeCoverModal = () => {
    showCoverModal.value = false;
};

const handleCreateCover = async () => {
    if (!coverForm.value.descripcion.trim()) {
        showToast("La descripción es obligatoria.");
        return;
    }
    if (coverForm.value.cantidad <= 0) {
        showToast("La cantidad debe ser mayor a 0.");
        return;
    }

    const cachedUserId = localStorage.getItem('userId');
    const userId = cachedUserId ? parseInt(cachedUserId, 10) : 0;
    if (userId <= 0) { showToast("No se encontró el usuario."); return; }

    isCreatingCover.value = true;
    try {
        const result = await createDeuda({
            id_usuario: userId,
            tipo_deuda: false,
            descripcion: coverForm.value.descripcion.trim(),
            cantidad: Number(coverForm.value.cantidad),
            fcha_registro: formatDate(new Date())
        });
        if (result === true) {
            showToast("Registrado correctamente", "success");
            closeCoverModal();
            dashboardData.value = null;
        } else {
            showToast("No se pudo registrar la cobertura.");
        }
    } catch (error) {
        showToast("Ocurrió un error al registrar la cobertura.");
    } finally {
        isCreatingCover.value = false;
    }
};
</script>

<template>
    <div class="debts-screen">
        <transition name="toast-fade">
            <div v-if="toast.visible" :class="['toast', `toast-${toast.type}`]" role="status" aria-live="polite">
                {{ toast.message }}
            </div>
        </transition>

        <transition name="modal-fade">
            <div v-if="showDebtModal" class="modal-overlay" @click.self="closeDebtModal">
                <div class="modal-content modal-content-expense">
                    <header class="modal-header modal-header-expense">
                        <h2>Agregar Deuda</h2>
                        <button class="modal-close" type="button" @click="closeDebtModal">&times;</button>
                    </header>
                    <div class="modal-body">
                        <div class="modal-field">
                            <label for="debt-desc">Descripción</label>
                            <input id="debt-desc" v-model="debtForm.descripcion" type="text" placeholder="Ej: Deuda COPPEL..." class="modal-input modal-input-expense" />
                        </div>
                        <div class="modal-field">
                            <label for="debt-amount">Cantidad</label>
                            <input id="debt-amount" v-model.number="debtForm.cantidad" type="number" min="0" step="0.01" placeholder="0.00" class="modal-input modal-input-expense" />
                        </div>
                    </div>
                    <footer class="modal-footer">
                        <button class="modal-cancel-btn" type="button" @click="closeDebtModal">Cancelar</button>
                        <button class="modal-submit-btn modal-submit-expense" type="button" @click="handleCreateDebt" :disabled="isCreatingDebt">
                            <span v-if="!isCreatingDebt">Registrar</span>
                            <span v-else class="button-spinner" aria-hidden="true"></span>
                        </button>
                    </footer>
                </div>
            </div>
        </transition>

        <transition name="modal-fade">
            <div v-if="showCoverModal" class="modal-overlay" @click.self="closeCoverModal">
                <div class="modal-content">
                    <header class="modal-header">
                        <h2>Agregar Cobertura</h2>
                        <button class="modal-close" type="button" @click="closeCoverModal">&times;</button>
                    </header>
                    <div class="modal-body">
                        <div class="modal-field">
                            <label for="cover-desc">Descripción</label>
                            <input id="cover-desc" v-model="coverForm.descripcion" type="text" placeholder="Ej: Pago parcial..." class="modal-input" />
                        </div>
                        <div class="modal-field">
                            <label for="cover-amount">Cantidad</label>
                            <input id="cover-amount" v-model.number="coverForm.cantidad" type="number" min="0" step="0.01" placeholder="0.00" class="modal-input" />
                        </div>
                    </div>
                    <footer class="modal-footer">
                        <button class="modal-cancel-btn" type="button" @click="closeCoverModal">Cancelar</button>
                        <button class="modal-submit-btn" type="button" @click="handleCreateCover" :disabled="isCreatingCover">
                            <span v-if="!isCreatingCover">Registrar</span>
                            <span v-else class="button-spinner" aria-hidden="true"></span>
                        </button>
                    </footer>
                </div>
            </div>
        </transition>

        <div class="sub-header">
            <h1>Deudas</h1>
            <div class="filters-row">
                <VueDatePicker
                    v-model="dateRange"
                    range
                    :enable-time-picker="false"
                    placeholder="Elige un rango"
                    class="custom-datepicker"
                    :teleport="false"
                    dark
                />
                <div class="selected-range">
                    <span>Desde: {{ startDate ? new Date(startDate).toLocaleDateString() : "-" }}</span>
                    <span>Hasta: {{ endDate ? new Date(endDate).toLocaleDateString() : "-" }}</span>
                </div>
                <button class="filters-button" type="button" @click="fetchDashboard" :disabled="isLoading">
                    <span v-if="!isLoading">Obtener Dashboard</span>
                    <span v-else class="button-spinner" aria-hidden="true"></span>
                </button>
                <button class="debt-button" type="button" @click="openDebtModal">Agregar Deuda</button>
                <button class="cover-button" type="button" @click="openCoverModal">Agregar Cobertura</button>
            </div>
        </div>

        <div class="dashboard-content">
            <div v-if="!dashboardData" class="preview-placeholder">
                <p>Selecciona un rango de fechas y presiona "Obtener Dashboard" para ver la información.</p>
            </div>

            <div v-else class="dashboard-grid">
                <section class="dashboard-card">
                    <header class="card-header">
                        <h2>Deudas</h2>
                        <span class="badge">{{ dashboardData.deudas?.length ?? 0 }}</span>
                    </header>
                    <div class="card-body">
                        <div v-if="!dashboardData.deudas || dashboardData.deudas.length === 0" class="empty-state">
                            No hay deudas en el rango seleccionado.
                        </div>
                        <ul v-else class="record-list">
                            <li v-for="deuda in dashboardData.deudas" :key="deuda.deuda_id" class="record-item">
                                <div class="record-main">
                                    <span class="record-name">{{ deuda.descripcion }}</span>
                                    <span class="record-category">{{ deuda.fcha_registro }}</span>
                                </div>
                                <span class="record-amount expense">-{{ formatCurrency(deuda.cantidad) }}</span>
                                <button class="delete-btn" type="button" @click="handleDeleteDeuda(deuda.deuda_id)">Eliminar</button>
                            </li>
                        </ul>
                    </div>
                </section>

                <section class="dashboard-card">
                    <header class="card-header">
                        <h2>Cubierto</h2>
                        <span class="badge">{{ dashboardData.cubierto?.length ?? 0 }}</span>
                    </header>
                    <div class="card-body">
                        <div v-if="!dashboardData.cubierto || dashboardData.cubierto.length === 0" class="empty-state">
                            No hay saldos cubiertos para mostrar.
                        </div>
                        <ul v-else class="record-list">
                            <li v-for="item in dashboardData.cubierto" :key="item.deuda_id" class="record-item">
                                <div class="record-main">
                                    <span class="record-name">{{ item.descripcion }}</span>
                                    <span class="record-category">{{ item.fcha_registro }}</span>
                                </div>
                                <span class="record-amount income">+{{ formatCurrency(item.cantidad) }}</span>
                                <button class="delete-btn" type="button" @click="handleDeleteDeuda(item.deuda_id)">Eliminar</button>
                            </li>
                        </ul>
                    </div>
                </section>

                <section class="dashboard-card stats-card">
                    <header class="card-header">
                        <h2>Estadísticas</h2>
                    </header>
                    <div class="card-body stats-body">
                        <div class="donut-wrapper">
                            <svg :viewBox="`0 0 ${donutRadius * 2 + 20} ${donutRadius * 2 + 20}`" class="donut">
                                <circle
                                    :cx="donutRadius + 10"
                                    :cy="donutRadius + 10"
                                    :r="donutRadius"
                                    class="donut-track"
                                />
                                <circle
                                    :cx="donutRadius + 10"
                                    :cy="donutRadius + 10"
                                    :r="donutRadius"
                                    class="donut-progress"
                                    :stroke-dasharray="donutCircumference"
                                    :stroke-dashoffset="donutDashOffset"
                                    :transform="`rotate(-90 ${donutRadius + 10} ${donutRadius + 10})`"
                                />
                                <text
                                    :x="donutRadius + 10"
                                    :y="donutRadius + 10"
                                    text-anchor="middle"
                                    dominant-baseline="central"
                                    class="donut-text"
                                >
                                    {{ donutPercent.toFixed(1) }}%
                                </text>
                            </svg>
                            <p class="donut-caption">Porcentaje cubierto</p>
                        </div>

                        <div class="stats-numbers">
                            <div class="stat-row">
                                <span class="stat-label">Porcentaje cubierto</span>
                                <span class="stat-value">{{ donutPercent.toFixed(2) }}%</span>
                            </div>
                            <div class="stat-row">
                                <span class="stat-label">Total cubierto</span>
                                <span class="stat-value income">{{ formatCurrency(dashboardData.totalSaldoCubierto) }}</span>
                            </div>
                            <div class="stat-row">
                                <span class="stat-label">Total deuda</span>
                                <span class="stat-value expense">{{ formatCurrency(dashboardData.totalSaldoDeuda) }}</span>
                            </div>
                            <div class="stat-row">
                                <span class="stat-label">Saldo pendiente</span>
                                <span class="stat-value" :class="saldoPendiente > 0 ? 'expense' : 'income'">
                                    {{ formatCurrency(saldoPendiente) }}
                                </span>
                            </div>
                        </div>
                    </div>
                </section>
            </div>
        </div>
    </div>
</template>

<style scoped>
.debts-screen {
    width: 100%;
    min-height: calc(100vh - 60px);
    display: flex;
    flex-direction: column;
    padding: 10px 16px;
    gap: 16px;
    position: relative;
}

.sub-header {
    display: flex;
    align-items: center;
    gap: 24px;
    background: rgba(15, 23, 42, 0.95);
    border: 1px solid rgba(44, 199, 124, 0.35);
    border-radius: 16px;
    padding: 16px 20px;
    box-shadow: 0 10px 30px rgba(15, 23, 42, 0.22);
    color: #ffffff;
    flex-wrap: wrap;
}

.sub-header h1 {
    margin: 0;
    font-size: 22px;
    font-weight: 700;
    line-height: 1.1;
}

.filters-row {
    display: flex;
    align-items: center;
    gap: 16px;
    flex: 1;
    flex-wrap: wrap;
}

.selected-range {
    display: flex;
    gap: 12px;
    font-weight: 600;
    color: var(--primary-color);
    font-size: 11px;
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

.filters-button {
    padding: 10px 20px;
    border: none;
    border-radius: 8px;
    background: linear-gradient(135deg, var(--primary-color), var(--secondary-color, #00d79a));
    color: #0f172a;
    font-weight: 700;
    font-size: 13px;
    cursor: pointer;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 40px;
    min-width: 160px;
}

.filters-button:disabled {
    opacity: 0.75;
    cursor: not-allowed;
    transform: none;
    box-shadow: none;
}

.filters-button:hover:not(:disabled) {
    transform: translateY(-1px);
    box-shadow: 0 6px 12px rgba(44, 199, 124, 0.25);
}

.button-spinner {
    width: 16px;
    height: 16px;
    border-radius: 50%;
    border: 2px solid rgba(44, 199, 124, 0.2);
    border-top-color: #0f172a;
    animation: spin 0.8s linear infinite;
    display: inline-block;
}

@keyframes spin {
    to { transform: rotate(360deg); }
}

.custom-datepicker {
    --dp-background-color: var(--vt-c-black);
    --dp-text-color: #ffffff;
    --dp-primary-color: var(--primary-color);
    --dp-primary-text-color: #ffffff;
    --dp-border-color: var(--primary-color);
    --dp-border-color-hover: #00d79a;
    --dp-hover-color: rgba(255, 255, 255, 0.15);
    --dp-hover-text-color: #ffffff;
    --dp-menu-border-color: var(--primary-color);
    --dp-border-radius: 8px;
    --dp-cell-border-radius: 6px;
    min-width: 240px;
}

.custom-datepicker :deep(.dp__input) {
    padding: 8px 10px;
    font-size: 12px;
    background: var(--vt-c-black);
}

.custom-datepicker :deep(.dp__menu) {
    background: var(--vt-c-black);
}

.dashboard-content {
    flex: 1;
    display: flex;
    flex-direction: column;
}

.preview-placeholder {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(15, 23, 42, 0.85);
    border: 1px dashed rgba(44, 199, 124, 0.35);
    border-radius: 16px;
    color: rgba(255, 255, 255, 0.7);
    padding: 24px;
    text-align: center;
    font-size: 14px;
    min-height: 300px;
}

.dashboard-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    grid-template-rows: auto auto;
    gap: 16px;
}

.dashboard-card {
    background: rgba(15, 23, 42, 0.95);
    border: 1px solid rgba(44, 199, 124, 0.35);
    border-radius: 16px;
    padding: 16px 18px;
    color: #ffffff;
    display: flex;
    flex-direction: column;
    min-height: 0;
    max-height: 400px;
    box-shadow: 0 10px 24px rgba(15, 23, 42, 0.22);
}

.stats-card {
    grid-column: 1 / -1;
}

.card-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 12px;
    padding-bottom: 8px;
    border-bottom: 1px solid rgba(44, 199, 124, 0.2);
}

.card-header h2 {
    margin: 0;
    font-size: 16px;
    font-weight: 700;
    color: var(--primary-color);
}

.badge {
    background: rgba(44, 199, 124, 0.15);
    color: var(--primary-color);
    font-size: 11px;
    font-weight: 700;
    padding: 2px 8px;
    border-radius: 10px;
    border: 1px solid rgba(44, 199, 124, 0.35);
}

.card-body {
    flex: 1;
    min-height: 0;
    overflow-y: auto;
}

.empty-state {
    color: rgba(255, 255, 255, 0.55);
    font-size: 13px;
    text-align: center;
    padding: 24px 8px;
}

.record-list {
    list-style: none;
    margin: 0;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.record-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 8px 10px;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
    border-radius: 8px;
    transition: background 0.15s ease;
}

.record-item:hover {
    background: rgba(255, 255, 255, 0.08);
}

.record-main {
    display: flex;
    flex-direction: column;
    gap: 2px;
    min-width: 0;
    flex: 1;
}

.record-name {
    font-size: 13px;
    font-weight: 600;
    color: #ffffff;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.record-category {
    font-size: 11px;
    color: rgba(255, 255, 255, 0.6);
}

.record-amount {
    font-size: 13px;
    font-weight: 700;
    white-space: nowrap;
}

.record-amount.expense,
.stat-value.expense {
    color: #ff6b6b;
}

.record-amount.income,
.stat-value.income {
    color: var(--primary-color);
}

.stats-body {
    display: flex;
    gap: 24px;
    align-items: flex-start;
    flex-wrap: wrap;
    overflow: visible;
}

.donut-wrapper {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
    flex-shrink: 0;
}

.donut {
    width: 160px;
    height: 160px;
}

.donut-track {
    fill: none;
    stroke: rgba(255, 255, 255, 0.08);
    stroke-width: 14;
}

.donut-progress {
    fill: none;
    stroke: var(--primary-color);
    stroke-width: 14;
    stroke-linecap: round;
    transition: stroke-dashoffset 0.6s ease;
}

.donut-text {
    fill: #ffffff;
    font-size: 22px;
    font-weight: 700;
}

.donut-caption {
    margin: 0;
    font-size: 12px;
    color: rgba(255, 255, 255, 0.7);
}

.stats-numbers {
    flex: 1;
    min-width: 220px;
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.stat-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
    border-radius: 8px;
}

.stat-label {
    font-size: 12px;
    color: rgba(255, 255, 255, 0.75);
    font-weight: 500;
}

.stat-value {
    font-size: 14px;
    font-weight: 700;
    color: #ffffff;
}

.delete-btn {
    padding: 4px 10px;
    border: none;
    border-radius: 6px;
    background: #ff4d4d;
    color: #ffffff;
    font-size: 11px;
    font-weight: 600;
    cursor: pointer;
    white-space: nowrap;
    transition: background 0.15s ease, transform 0.15s ease;
}

.delete-btn:hover {
    background: #e63939;
    transform: translateY(-1px);
}

.delete-btn:active {
    transform: translateY(0);
}

.debt-button {
    padding: 10px 20px;
    border: none;
    border-radius: 8px;
    background: linear-gradient(135deg, #ff6b6b, #e03131);
    color: #ffffff;
    font-weight: 700;
    font-size: 13px;
    cursor: pointer;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
    min-height: 40px;
}

.debt-button:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 12px rgba(255, 107, 107, 0.3);
}

.cover-button {
    padding: 10px 20px;
    border: none;
    border-radius: 8px;
    background: linear-gradient(135deg, #4dabf7, #228be6);
    color: #ffffff;
    font-weight: 700;
    font-size: 13px;
    cursor: pointer;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
    min-height: 40px;
}

.cover-button:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 12px rgba(77, 171, 247, 0.3);
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
    width: min(90%, 400px);
    box-shadow: 0 20px 50px rgba(0, 0, 0, 0.4);
    color: #ffffff;
}

.modal-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 20px;
    padding-bottom: 12px;
    border-bottom: 1px solid rgba(77, 171, 247, 0.3);
}

.modal-header h2 {
    margin: 0;
    font-size: 18px;
    font-weight: 700;
    color: #4dabf7;
}

.modal-close {
    background: none;
    border: none;
    color: rgba(255, 255, 255, 0.6);
    font-size: 24px;
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
    gap: 16px;
}

.modal-field {
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.modal-field label {
    font-size: 13px;
    font-weight: 600;
    color: rgba(255, 255, 255, 0.85);
}

.modal-input {
    padding: 10px 12px;
    border-radius: 8px;
    border: 1px solid rgba(77, 171, 247, 0.4);
    background: rgba(0, 0, 0, 0.4);
    color: #ffffff;
    font-size: 14px;
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
    margin-top: 20px;
}

.modal-cancel-btn {
    padding: 8px 16px;
    border: 1px solid rgba(255, 255, 255, 0.2);
    border-radius: 8px;
    background: transparent;
    color: rgba(255, 255, 255, 0.8);
    font-size: 13px;
    font-weight: 600;
    cursor: pointer;
    transition: background 0.15s ease;
}

.modal-cancel-btn:hover {
    background: rgba(255, 255, 255, 0.08);
}

.modal-submit-btn {
    padding: 8px 20px;
    border: none;
    border-radius: 8px;
    background: linear-gradient(135deg, #4dabf7, #228be6);
    color: #ffffff;
    font-size: 13px;
    font-weight: 700;
    cursor: pointer;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
    display: flex;
    align-items: center;
    justify-content: center;
    min-width: 90px;
}

.modal-submit-btn:disabled {
    opacity: 0.75;
    cursor: not-allowed;
}

.modal-submit-btn:hover:not(:disabled) {
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(77, 171, 247, 0.3);
}

.modal-content-expense {
    border-color: rgba(255, 107, 107, 0.5);
}

.modal-header-expense {
    border-bottom-color: rgba(255, 107, 107, 0.3);
}

.modal-header-expense h2 {
    color: #ff6b6b;
}

.modal-input-expense {
    border-color: rgba(255, 107, 107, 0.4);
}

.modal-input-expense:focus {
    border-color: #ff6b6b;
}

.modal-submit-expense {
    background: linear-gradient(135deg, #ff6b6b, #e03131);
}

.modal-submit-expense:hover:not(:disabled) {
    box-shadow: 0 4px 12px rgba(255, 107, 107, 0.3);
}

.modal-fade-enter-active,
.modal-fade-leave-active {
    transition: opacity 0.2s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
    opacity: 0;
}

@media (max-width: 960px) {
    .sub-header {
        flex-direction: column;
        align-items: flex-start;
    }

    .filters-row {
        width: 100%;
        flex-direction: column;
        align-items: stretch;
    }

    .dashboard-grid {
        grid-template-columns: 1fr;
    }
}
</style>
