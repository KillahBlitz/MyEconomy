<script setup>
import { computed, onMounted, ref } from "vue";
import { VueDatePicker } from "@vuepic/vue-datepicker";
import "@vuepic/vue-datepicker/dist/main.css";
import { getCategories, getDashBoardContability, deleteSaldo, createSaldo } from "../../services/api.js";

const categoriesDictionary = ref({});
const dateRange = ref([null, null]);
const selectedCategories = ref([]);
const hideIncome = ref(false);
const isDropdownOpen = ref(false);
const isLoading = ref(false);
const dashboardData = ref(null);

const showIncomeModal = ref(false);
const incomeForm = ref({ concepto: "", cantidad: 0 });
const isCreatingIncome = ref(false);

const showExpenseModal = ref(false);
const expenseForm = ref({ concepto: "", cantidad: 0, id_categoria: null });
const isCreatingExpense = ref(false);
const isExpenseCategoryOpen = ref(false);

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

const startDate = computed(() => dateRange.value?.[0] ?? null);
const endDate = computed(() => dateRange.value?.[1] ?? null);

const currencyFormatter = new Intl.NumberFormat("es-MX", {
    style: "currency",
    currency: "MXN",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
});
const formatCurrency = (amount) => currencyFormatter.format(Number(amount) || 0);

const getCategoryName = (id) => {
    if (id == null) return "Sin categoria";
    return categoriesDictionary.value[String(id)] ?? "Sin categoria";
};

const totalDeuda = computed(() => {
    if (!dashboardData.value) return 0;
    const diff = (dashboardData.value.totalSaldoGasto ?? 0) - (dashboardData.value.totalSaldoIngreso ?? 0);
    return diff > 0 ? diff : 0;
});

const donutRadius = 60;
const donutCircumference = computed(() => 2 * Math.PI * donutRadius);
const donutPercent = computed(() => {
    const raw = dashboardData.value?.porcentajeGasto ?? 0;
    return Math.max(0, Math.min(100, raw));
});
const donutDashOffset = computed(() => {
    return donutCircumference.value * (1 - donutPercent.value / 100);
});

const CATEGORY_COLORS = [
    "#2cc77c", "#4dabf7", "#f59f00", "#f06595", "#845ef7",
    "#20c997", "#ff922b", "#e64980", "#15aabf", "#fab005",
    "#94d82d", "#a78bfa"
];

const categoryBreakdown = computed(() => {
    if (!dashboardData.value) return [];
    const gastos = dashboardData.value.saldoGastos ?? [];
    const total = Number(dashboardData.value.totalSaldoGasto) || 0;
    if (total <= 0 || gastos.length === 0) return [];

    const map = new Map();
    for (const g of gastos) {
        const key = g.id_categoria ?? "sin-categoria";
        const current = map.get(key) ?? {
            id: g.id_categoria,
            key,
            name: getCategoryName(g.id_categoria),
            amount: 0,
        };
        current.amount += Number(g.cantidad) || 0;
        map.set(key, current);
    }

    const items = Array.from(map.values()).sort((a, b) => b.amount - a.amount);

    let acc = 0;
    return items.map((item, idx) => {
        const percent = (item.amount / total) * 100;
        const startPercent = acc;
        acc += percent;
        return {
            ...item,
            percent,
            startPercent,
            color: CATEGORY_COLORS[idx % CATEGORY_COLORS.length],
        };
    });
});

const loadCategories = async () => {
    const categories = await getCategories();

    categoriesDictionary.value = Object.fromEntries(
        (Array.isArray(categories) ? categories : []).map((category) => [String(category.categoria_id), category.tipo_categoria])
    );
};

const toggleDropdown = () => {
    isDropdownOpen.value = !isDropdownOpen.value;
};

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
            endDate: formatDate(dateRange.value[1]),
            neededSaldo: !hideIncome.value,
            categories: selectedCategories.value
        };

        const data = await getDashBoardContability(params);
        console.log("Datos del Dashboard recibidos:", data);

        if (data && data.success === false) {
            dashboardData.value = null;
            showToast(data.error || "No se pudo obtener el dashboard.");
            return;
        }

        dashboardData.value = data;

    } catch (error) {
        console.error("Error al obtener el dashboard:", error);
        dashboardData.value = null;
        showToast("Ocurrió un error al obtener el dashboard.");
    } finally {
        isLoading.value = false;
    }
};

const handleDeleteSaldo = async (saldoId) => {
    try {
        const result = await deleteSaldo(saldoId);
        if (result === true) {
            if (dashboardData.value) {
                dashboardData.value.saldoGastos = dashboardData.value.saldoGastos?.filter(s => s.saldo_id !== saldoId) ?? [];
                dashboardData.value.saldoIngresos = dashboardData.value.saldoIngresos?.filter(s => s.saldo_id !== saldoId) ?? [];
            }
            showToast("Registro eliminado correctamente", "success");
        } else {
            showToast("No se pudo eliminar el registro.");
        }
    } catch (error) {
        console.error("Error al eliminar saldo:", error);
        showToast("Ocurrió un error al eliminar el registro.");
    }
};

const openIncomeModal = () => {
    incomeForm.value = { concepto: "", cantidad: 0 };
    showIncomeModal.value = true;
};

const closeIncomeModal = () => {
    showIncomeModal.value = false;
};

const handleCreateIncome = async () => {
    if (!incomeForm.value.concepto.trim()) {
        showToast("El concepto es obligatorio.");
        return;
    }
    if (incomeForm.value.cantidad <= 0) {
        showToast("La cantidad debe ser mayor a 0.");
        return;
    }

    const cachedUserId = localStorage.getItem('userId');
    const userId = cachedUserId ? parseInt(cachedUserId, 10) : 0;

    if (userId <= 0) {
        showToast("No se encontró el usuario.");
        return;
    }

    isCreatingIncome.value = true;

    try {
        const result = await createSaldo({
            concepto: incomeForm.value.concepto.trim(),
            cantidad: Number(incomeForm.value.cantidad),
            id_usuario: userId,
            id_categoria: 0
        });

        if (result === true) {
            showToast("Registrado correctamente", "success");
            closeIncomeModal();
            dashboardData.value = null;
        } else {
            showToast("No se pudo registrar el ingreso.");
        }
    } catch (error) {
        console.error("Error al crear ingreso:", error);
        showToast("Ocurrió un error al registrar el ingreso.");
    } finally {
        isCreatingIncome.value = false;
    }
};

const selectableCategories = computed(() => {
    return Object.fromEntries(
        Object.entries(categoriesDictionary.value).filter(([id]) => id !== "2")
    );
});

const openExpenseModal = () => {
    expenseForm.value = { concepto: "", cantidad: 0, id_categoria: null };
    isExpenseCategoryOpen.value = false;
    showExpenseModal.value = true;
};

const closeExpenseModal = () => {
    showExpenseModal.value = false;
};

const toggleExpenseCategoryDropdown = () => {
    isExpenseCategoryOpen.value = !isExpenseCategoryOpen.value;
};

const selectExpenseCategory = (id) => {
    expenseForm.value.id_categoria = parseInt(id);
    isExpenseCategoryOpen.value = false;
};

const handleCreateExpense = async () => {
    if (!expenseForm.value.concepto.trim()) {
        showToast("El concepto es obligatorio.");
        return;
    }
    if (expenseForm.value.cantidad <= 0) {
        showToast("La cantidad debe ser mayor a 0.");
        return;
    }
    if (expenseForm.value.id_categoria == null) {
        showToast("Selecciona una categoría.");
        return;
    }

    const cachedUserId = localStorage.getItem('userId');
    const userId = cachedUserId ? parseInt(cachedUserId, 10) : 0;

    if (userId <= 0) {
        showToast("No se encontró el usuario.");
        return;
    }

    isCreatingExpense.value = true;

    try {
        const result = await createSaldo({
            concepto: expenseForm.value.concepto.trim(),
            cantidad: Number(expenseForm.value.cantidad),
            id_usuario: userId,
            id_categoria: expenseForm.value.id_categoria
        });

        if (result === true) {
            showToast("Registrado correctamente", "success");
            closeExpenseModal();
            dashboardData.value = null;
        } else {
            showToast("No se pudo registrar el gasto.");
        }
    } catch (error) {
        console.error("Error al crear gasto:", error);
        showToast("Ocurrió un error al registrar el gasto.");
    } finally {
        isCreatingExpense.value = false;
    }
};

onMounted(() => {
    loadCategories();
});
</script>

<template>
    <div class="dashboard-screen">
        <transition name="toast-fade">
            <div v-if="toast.visible" :class="['toast', `toast-${toast.type}`]" role="status" aria-live="polite">
                {{ toast.message }}
            </div>
        </transition>

        <transition name="modal-fade">
            <div v-if="showIncomeModal" class="modal-overlay" @click.self="closeIncomeModal">
                <div class="modal-content">
                    <header class="modal-header">
                        <h2>Agregar Ingreso</h2>
                        <button class="modal-close" type="button" @click="closeIncomeModal">&times;</button>
                    </header>
                    <div class="modal-body">
                        <div class="modal-field">
                            <label for="income-concepto">Concepto</label>
                            <input
                                id="income-concepto"
                                v-model="incomeForm.concepto"
                                type="text"
                                placeholder="Ej: Salario, Freelance..."
                                class="modal-input"
                            />
                        </div>
                        <div class="modal-field">
                            <label for="income-cantidad">Cantidad</label>
                            <input
                                id="income-cantidad"
                                v-model.number="incomeForm.cantidad"
                                type="number"
                                min="0"
                                step="0.01"
                                placeholder="0.00"
                                class="modal-input"
                            />
                        </div>
                    </div>
                    <footer class="modal-footer">
                        <button class="modal-cancel-btn" type="button" @click="closeIncomeModal">Cancelar</button>
                        <button class="modal-submit-btn" type="button" @click="handleCreateIncome" :disabled="isCreatingIncome">
                            <span v-if="!isCreatingIncome">Registrar</span>
                            <span v-else class="button-spinner" aria-hidden="true"></span>
                        </button>
                    </footer>
                </div>
            </div>
        </transition>

        <transition name="modal-fade">
            <div v-if="showExpenseModal" class="modal-overlay" @click.self="closeExpenseModal">
                <div class="modal-content modal-content-expense">
                    <header class="modal-header modal-header-expense">
                        <h2>Agregar Gasto</h2>
                        <button class="modal-close" type="button" @click="closeExpenseModal">&times;</button>
                    </header>
                    <div class="modal-body">
                        <div class="modal-field">
                            <label for="expense-concepto">Concepto</label>
                            <input
                                id="expense-concepto"
                                v-model="expenseForm.concepto"
                                type="text"
                                placeholder="Ej: Renta, Comida..."
                                class="modal-input modal-input-expense"
                            />
                        </div>
                        <div class="modal-field">
                            <label for="expense-cantidad">Cantidad</label>
                            <input
                                id="expense-cantidad"
                                v-model.number="expenseForm.cantidad"
                                type="number"
                                min="0"
                                step="0.01"
                                placeholder="0.00"
                                class="modal-input modal-input-expense"
                            />
                        </div>
                        <div class="modal-field">
                            <label>Categoría</label>
                            <div class="modal-select-container">
                                <div class="modal-select-trigger" @click="toggleExpenseCategoryDropdown">
                                    <span>
                                        {{ expenseForm.id_categoria == null 
                                            ? 'Seleccionar categoría...' 
                                            : getCategoryName(expenseForm.id_categoria) 
                                        }}
                                    </span>
                                    <span class="arrow" :class="{ open: isExpenseCategoryOpen }">▼</span>
                                </div>
                                <div class="modal-select-dropdown" v-show="isExpenseCategoryOpen">
                                    <div
                                        v-for="(name, id) in selectableCategories"
                                        :key="id"
                                        class="modal-dropdown-item"
                                        :class="{ selected: expenseForm.id_categoria === parseInt(id) }"
                                        @click="selectExpenseCategory(id)"
                                    >
                                        {{ name }}
                                    </div>
                                    <div v-if="Object.keys(selectableCategories).length === 0" class="no-options">
                                        Cargando...
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <footer class="modal-footer">
                        <button class="modal-cancel-btn" type="button" @click="closeExpenseModal">Cancelar</button>
                        <button class="modal-submit-btn modal-submit-expense" type="button" @click="handleCreateExpense" :disabled="isCreatingExpense">
                            <span v-if="!isCreatingExpense">Registrar</span>
                            <span v-else class="button-spinner" aria-hidden="true"></span>
                        </button>
                    </footer>
                </div>
            </div>
        </transition>

        <div class="filters-panel">
            <div class="panel-header">
                <h1>Contabilidad</h1>
                <p class="calendar-helper">Filtra por rango, categorías e ingresos.</p>
            </div>

            <div class="filters-content">
                <div class="filter-block">
                    <label>Rango de fechas</label>
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
                </div>

                <div class="filter-block">
                    <label>Categorías</label>
                    <div class="custom-select-container">
                        <div class="select-trigger" @click="toggleDropdown">
                            <span>
                                {{ selectedCategories.length === 0 
                                    ? 'Seleccionar...' 
                                    : `${selectedCategories.length} seleccionadas` 
                                }}
                            </span>
                            <span class="arrow" :class="{ open: isDropdownOpen }">▼</span>
                        </div>
                        
                        <div class="select-dropdown" v-show="isDropdownOpen">
                            <label v-for="(name, id) in categoriesDictionary" :key="id" class="dropdown-item">
                                <input type="checkbox" :value="parseInt(id)" v-model="selectedCategories" />
                                <span>{{ name }}</span>
                            </label>
                            <div v-if="Object.keys(categoriesDictionary).length === 0" class="no-options">
                                Cargando...
                            </div>
                        </div>
                    </div>
                </div>

                <label class="income-check">
                    <input v-model="hideIncome" type="checkbox" />
                    <span>Sin ver ingreso</span>
                </label>
            </div>

            <div class="panel-footer">
                <button class="income-button" type="button" @click="openIncomeModal">
                    Agregar Ingreso
                </button>
                <button class="expense-button" type="button" @click="openExpenseModal">
                    Agregar Gasto
                </button>
                <button class="filters-button" type="button" @click="fetchDashboard" :disabled="isLoading">
                    <span v-if="!isLoading">Obtener Dashboard</span>
                    <span v-else class="button-spinner" aria-hidden="true"></span>
                </button>
            </div>
        </div>

        <div class="dashboard-preview">
            <div v-if="!dashboardData" class="preview-placeholder">
                <p>Selecciona un rango de fechas y presiona "Obtener Dashboard" para ver la información.</p>
            </div>

            <div v-else class="dashboard-grid">
                <section class="dashboard-card">
                    <header class="card-header">
                        <h2>Gastos</h2>
                        <span class="badge">{{ dashboardData.saldoGastos?.length ?? 0 }}</span>
                    </header>

                    <div class="card-body">
                        <div v-if="!dashboardData.saldoGastos || dashboardData.saldoGastos.length === 0" class="empty-state">
                            No hay gastos en el rango seleccionado.
                        </div>
                        <ul v-else class="record-list">
                            <li v-for="gasto in dashboardData.saldoGastos" :key="gasto.saldo_id" class="record-item">
                                <div class="record-main">
                                    <span class="record-name">{{ gasto.concepto }}</span>
                                    <span class="record-category">{{ getCategoryName(gasto.id_categoria) }}</span>
                                </div>
                                <span class="record-amount expense">-{{ formatCurrency(gasto.cantidad) }}</span>
                                <button v-if="gasto.id_categoria !== 2" class="delete-btn" type="button" @click="handleDeleteSaldo(gasto.saldo_id)">Eliminar</button>
                            </li>
                        </ul>
                    </div>
                </section>

                <section class="dashboard-card">
                    <header class="card-header">
                        <h2>Ingresos</h2>
                        <span class="badge">{{ dashboardData.saldoIngresos?.length ?? 0 }}</span>
                    </header>

                    <div class="card-body">
                        <div v-if="!dashboardData.saldoIngresos || dashboardData.saldoIngresos.length === 0" class="empty-state">
                            No hay ingresos para mostrar.
                        </div>
                        <ul v-else class="record-list">
                            <li v-for="ingreso in dashboardData.saldoIngresos" :key="ingreso.saldo_id" class="record-item">
                                <div class="record-main">
                                    <span class="record-name">{{ ingreso.concepto }}</span>
                                </div>
                                <span class="record-amount income">+{{ formatCurrency(ingreso.cantidad) }}</span>
                                <button class="delete-btn" type="button" @click="handleDeleteSaldo(ingreso.saldo_id)">Eliminar</button>
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
                            <p class="donut-caption">Porcentaje de gasto</p>
                        </div>

                        <div class="donut-wrapper">
                            <svg :viewBox="`0 0 ${donutRadius * 2 + 20} ${donutRadius * 2 + 20}`" class="donut">
                                <circle
                                    :cx="donutRadius + 10"
                                    :cy="donutRadius + 10"
                                    :r="donutRadius"
                                    class="donut-track"
                                />
                                <template v-if="categoryBreakdown.length > 0">
                                    <circle
                                        v-for="seg in categoryBreakdown"
                                        :key="seg.key"
                                        :cx="donutRadius + 10"
                                        :cy="donutRadius + 10"
                                        :r="donutRadius"
                                        class="donut-segment"
                                        :stroke="seg.color"
                                        :stroke-dasharray="`${(seg.percent / 100) * donutCircumference} ${donutCircumference}`"
                                        :transform="`rotate(${-90 + (seg.startPercent / 100) * 360} ${donutRadius + 10} ${donutRadius + 10})`"
                                    />
                                </template>
                                <text
                                    v-if="categoryBreakdown.length > 0"
                                    :x="donutRadius + 10"
                                    :y="donutRadius + 10"
                                    text-anchor="middle"
                                    dominant-baseline="central"
                                    class="donut-text-small"
                                >
                                    {{ categoryBreakdown.length }}
                                    <tspan :x="donutRadius + 10" dy="14" class="donut-subtext">
                                        {{ categoryBreakdown.length === 1 ? 'categoria' : 'categorias' }}
                                    </tspan>
                                </text>
                            </svg>
                            <p class="donut-caption">Gasto por categoria</p>

                            <ul v-if="categoryBreakdown.length > 0" class="category-legend">
                                <li v-for="seg in categoryBreakdown" :key="seg.key" class="legend-item">
                                    <span class="legend-color" :style="{ background: seg.color }"></span>
                                    <span class="legend-name" :title="seg.name">{{ seg.name }}</span>
                                    <span class="legend-values">
                                        <span class="legend-amount">{{ formatCurrency(seg.amount) }}</span>
                                        <span class="legend-percent">{{ seg.percent.toFixed(1) }}%</span>
                                    </span>
                                </li>
                            </ul>
                            <p v-else class="empty-state legend-empty">Sin gastos para segmentar.</p>
                        </div>

                        <div class="stats-numbers">
                            <div class="stat-row">
                                <span class="stat-label">Porcentaje de gasto</span>
                                <span class="stat-value">{{ donutPercent.toFixed(2) }}%</span>
                            </div>
                            <div class="stat-row">
                                <span class="stat-label">Total de ingreso</span>
                                <span class="stat-value income">{{ formatCurrency(dashboardData.totalSaldoIngreso) }}</span>
                            </div>
                            <div class="stat-row">
                                <span class="stat-label">Total de gasto</span>
                                <span class="stat-value expense">{{ formatCurrency(dashboardData.totalSaldoGasto) }}</span>
                            </div>
                            <div class="stat-row">
                                <span class="stat-label">Total de deuda</span>
                                <span class="stat-value" :class="totalDeuda > 0 ? 'expense' : 'income'">
                                    {{ formatCurrency(totalDeuda) }}
                                </span>
                            </div>
                            <div class="stat-row">
                                <span class="stat-label">Dinero Disponible</span>
                                <span class="stat-value income">
                                    {{ formatCurrency(Math.max(0, (dashboardData.totalSaldoIngreso ?? 0) - (dashboardData.totalSaldoGasto ?? 0))) }}
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

.dashboard-screen {
    width: 100%;
    min-height: calc(100vh - 60px);
    display: flex;
    justify-content: flex-start;
    align-items: stretch;
    padding: 10px 16px;
    gap: 20px;
    position: relative;
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

.filters-panel {
    width: min(100%, 240px);
    height: calc(100vh - 100px);
    display: flex;
    flex-direction: column;
    background: rgba(15, 23, 42, 0.95);
    color: #ffffff;
    border: 1px solid rgba(44, 199, 124, 0.35);
    border-radius: 16px;
    padding: 20px 16px;
    box-shadow: 0 10px 30px rgba(15, 23, 42, 0.22);
}

.filters-content {
    flex-grow: 1;
    margin-top: 16px;
}

.panel-footer {
    margin-top: auto;
}

.dashboard-preview {
    flex: 1;
    min-width: 0;
}

.filters-panel h1 {
    margin-bottom: 2px;
    font-size: 22px;
    font-weight: 700;
    color: inherit;
    line-height: 1.1;
}

.calendar-helper {
    margin-top: 4px;
    font-size: 12px;
    color: rgba(255, 255, 255, 0.78);
    line-height: 1.3;
}

.filter-block {
    display: flex;
    flex-direction: column;
    gap: 6px;
    margin-bottom: 24px;
}

.filter-block label,
.income-check {
    font-size: 13px;
    font-weight: 600;
    color: #ffffff;
}

.selected-range {
    display: flex;
    flex-direction: column;
    gap: 4px;
    margin-top: 4px;
    font-weight: 600;
    color: var(--primary-color);
    font-size: 11px;
}

.custom-select-container {
    position: relative;
    width: 100%;
}

.select-trigger {
    display: flex;
    justify-content: space-between;
    align-items: center;
    background: var(--vt-c-black);
    border: 1px solid var(--primary-color);
    padding: 8px 10px; 
    border-radius: 8px;
    color: #ffffff;
    font-size: 12px;
    cursor: pointer;
    user-select: none;
    transition: all 0.2s ease;
}

.select-trigger:hover {
    border-color: #00d79a;
}

.arrow {
    font-size: 9px;
    transition: transform 0.3s ease;
}

.arrow.open {
    transform: rotate(180deg);
}

.select-dropdown {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    background: var(--vt-c-black);
    border: 1px solid var(--primary-color);
    border-radius: 8px;
    margin-top: 4px;
    max-height: 160px;
    overflow-y: auto;
    z-index: 50;
    display: flex;
    flex-direction: column;
    padding: 4px 0;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
}

.dropdown-item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 8px 10px;
    cursor: pointer;
    font-size: 12px;
    color: #ffffff;
    transition: background 0.15s ease;
}

.dropdown-item:hover {
    background: rgba(255, 255, 255, 0.1);
}

.dropdown-item input {
    accent-color: var(--primary-color);
    width: 14px;
    height: 14px;
    cursor: pointer;
}

.no-options {
    padding: 8px;
    font-size: 12px;
    color: rgba(255, 255, 255, 0.5);
    text-align: center;
}

.income-check {
    display: flex;
    align-items: center;
    gap: 8px;
    cursor: pointer;
}

.income-check input {
    accent-color: var(--primary-color);
    width: 14px;
    height: 14px;
}

.filters-button {
    width: 100%;
    padding: 10px;
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
    to {
        transform: rotate(360deg);
    }
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
}

.custom-datepicker :deep(.dp__input) {
    padding: 8px 10px;
    font-size: 12px;
    background: var(--vt-c-black);
}

.custom-datepicker :deep(.dp__menu) {
    background: var(--vt-c-black);
}

.dashboard-preview {
    display: flex;
    flex-direction: column;
    min-height: calc(100vh - 100px);
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
}

.dashboard-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    grid-template-rows: auto auto;
    gap: 16px;
    height: calc(100vh - 100px);
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

.donut-segment {
    fill: none;
    stroke-width: 14;
    stroke-linecap: butt;
    transition: stroke-dasharray 0.6s ease;
}

.donut-text {
    fill: #ffffff;
    font-size: 22px;
    font-weight: 700;
}

.donut-text-small {
    fill: #ffffff;
    font-size: 18px;
    font-weight: 700;
}

.donut-subtext {
    fill: rgba(255, 255, 255, 0.6);
    font-size: 9px;
    font-weight: 500;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.donut-caption {
    margin: 0;
    font-size: 12px;
    color: rgba(255, 255, 255, 0.7);
}

.category-legend {
    list-style: none;
    margin: 4px 0 0;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: 4px;
    width: 100%;
    max-width: 240px;
    max-height: 140px;
    overflow-y: auto;
}

.legend-item {
    display: grid;
    grid-template-columns: 12px 1fr auto;
    align-items: center;
    gap: 8px;
    font-size: 11px;
}

.legend-color {
    width: 12px;
    height: 12px;
    border-radius: 3px;
    display: inline-block;
}

.legend-name {
    color: rgba(255, 255, 255, 0.85);
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.legend-values {
    display: flex;
    align-items: center;
    gap: 8px;
    white-space: nowrap;
}

.legend-amount {
    color: #ff6b6b;
    font-weight: 700;
}

.legend-percent {
    color: rgba(255, 255, 255, 0.7);
    font-weight: 600;
    min-width: 42px;
    text-align: right;
}

.legend-empty {
    padding: 8px;
    font-size: 12px;
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

.income-button {
    width: 100%;
    padding: 10px;
    border: none;
    border-radius: 8px;
    background: linear-gradient(135deg, #4dabf7, #228be6);
    color: #ffffff;
    font-weight: 700;
    font-size: 13px;
    cursor: pointer;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 40px;
    margin-bottom: 10px;
}

.income-button:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 12px rgba(77, 171, 247, 0.3);
}

.expense-button {
    width: 100%;
    padding: 10px;
    border: none;
    border-radius: 8px;
    background: linear-gradient(135deg, #ff6b6b, #e03131);
    color: #ffffff;
    font-weight: 700;
    font-size: 13px;
    cursor: pointer;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 40px;
    margin-bottom: 10px;
}

.expense-button:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 12px rgba(255, 107, 107, 0.3);
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

.modal-select-container {
    position: relative;
    width: 100%;
}

.modal-select-trigger {
    display: flex;
    justify-content: space-between;
    align-items: center;
    background: rgba(0, 0, 0, 0.4);
    border: 1px solid rgba(255, 107, 107, 0.4);
    padding: 10px 12px;
    border-radius: 8px;
    color: #ffffff;
    font-size: 14px;
    cursor: pointer;
    user-select: none;
    transition: border-color 0.2s ease;
}

.modal-select-trigger:hover {
    border-color: #ff6b6b;
}

.modal-select-dropdown {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    background: rgba(15, 23, 42, 0.98);
    border: 1px solid rgba(255, 107, 107, 0.4);
    border-radius: 8px;
    margin-top: 4px;
    max-height: 160px;
    overflow-y: auto;
    z-index: 210;
    display: flex;
    flex-direction: column;
    padding: 4px 0;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
}

.modal-dropdown-item {
    padding: 10px 12px;
    cursor: pointer;
    font-size: 13px;
    color: #ffffff;
    transition: background 0.15s ease;
}

.modal-dropdown-item:hover {
    background: rgba(255, 255, 255, 0.1);
}

.modal-dropdown-item.selected {
    background: rgba(255, 107, 107, 0.2);
    color: #ff6b6b;
    font-weight: 600;
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
    .dashboard-screen {
        flex-direction: column;
        height: auto;
    }

    .filters-panel {
        width: 100%;
        height: auto;
    }

    .dashboard-preview {
        min-height: auto;
    }

    .dashboard-grid {
        grid-template-columns: 1fr;
        height: auto;
    }

    .stats-body {
        justify-content: center;
    }
}
</style>