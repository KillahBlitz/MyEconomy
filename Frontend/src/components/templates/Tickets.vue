<script setup>
import { onMounted, ref } from "vue";
import { getTickets, deleteTicket, getTicketDetail, createTicket } from "../../services/api.js";

const tickets = ref([]);
const isLoading = ref(false);

const toast = ref({ visible: false, message: "", type: "error" });
let toastTimer = null;
const showToast = (message, type = "error") => {
    if (toastTimer) clearTimeout(toastTimer);
    toast.value = { visible: true, message, type };
    toastTimer = setTimeout(() => { toast.value.visible = false; }, 3000);
};

const currencyFormatter = new Intl.NumberFormat("es-MX", {
    style: "currency", currency: "MXN", minimumFractionDigits: 2, maximumFractionDigits: 2,
});
const formatCurrency = (amount) => currencyFormatter.format(Number(amount) || 0);

const showDeleteConfirm = ref(false);
const ticketToDelete = ref(null);
const isDeleting = ref(false);

const showDetailModal = ref(false);
const ticketDetail = ref(null);
const isLoadingDetail = ref(false);

const showCreateModal = ref(false);
const productRows = ref([]);
const isCreating = ref(false);

const loadTickets = async () => {
    isLoading.value = true;
    try {
        const data = await getTickets();
        tickets.value = Array.isArray(data) ? data : [];
    } catch {
        tickets.value = [];
        showToast("Error al cargar tickets.");
    } finally {
        isLoading.value = false;
    }
};

const openDeleteConfirm = (ticketId) => {
    ticketToDelete.value = ticketId;
    showDeleteConfirm.value = true;
};

const closeDeleteConfirm = () => {
    showDeleteConfirm.value = false;
    ticketToDelete.value = null;
};

const confirmDelete = async () => {
    if (!ticketToDelete.value) return;
    isDeleting.value = true;
    try {
        const result = await deleteTicket(ticketToDelete.value);
        if (result === true) {
            tickets.value = tickets.value.filter(t => t.ticket_id !== ticketToDelete.value);
            showToast("Ticket Eliminado", "success");
        } else {
            showToast("No se pudo eliminar el ticket.");
        }
    } catch {
        showToast("Error al eliminar el ticket.");
    } finally {
        isDeleting.value = false;
        closeDeleteConfirm();
    }
};

const openDetail = async (ticketId) => {
    isLoadingDetail.value = true;
    showDetailModal.value = true;
    ticketDetail.value = null;
    try {
        const data = await getTicketDetail(ticketId);
        ticketDetail.value = data;
    } catch {
        showToast("Error al cargar detalle.");
        showDetailModal.value = false;
    } finally {
        isLoadingDetail.value = false;
    }
};

const closeDetail = () => {
    showDetailModal.value = false;
    ticketDetail.value = null;
};

const openCreateModal = () => {
    productRows.value = [{ descripcion: "", cantidad: 1, precio_unitario: 0 }];
    showCreateModal.value = true;
};

const closeCreateModal = () => {
    showCreateModal.value = false;
};

const addProductRow = () => {
    productRows.value.push({ descripcion: "", cantidad: 1, precio_unitario: 0 });
};

const removeProductRow = (index) => {
    if (productRows.value.length > 1) {
        productRows.value.splice(index, 1);
    }
};

const handleCreateTicket = async () => {
    const valid = productRows.value.every(p => p.descripcion.trim() && p.cantidad > 0 && p.precio_unitario > 0);
    if (!valid) {
        showToast("Completa todos los productos correctamente.");
        return;
    }

    isCreating.value = true;
    try {
        const products = productRows.value.map(p => ({
            descripcion: p.descripcion.trim(),
            cantidad: Number(p.cantidad),
            precio_unitario: Number(p.precio_unitario)
        }));
        const result = await createTicket(products);
        if (result === true) {
            showToast("Registrado correctamente", "success");
            closeCreateModal();
            await loadTickets();
        } else {
            showToast("No se pudo crear el ticket.");
        }
    } catch {
        showToast("Error al crear el ticket.");
    } finally {
        isCreating.value = false;
    }
};

onMounted(() => {
    loadTickets();
});
</script>

<template>
    <div class="tickets-screen">
        <transition name="toast-fade">
            <div v-if="toast.visible" :class="['toast', `toast-${toast.type}`]" role="status" aria-live="polite">
                {{ toast.message }}
            </div>
        </transition>

        <transition name="modal-fade">
            <div v-if="showDeleteConfirm" class="modal-overlay" @click.self="closeDeleteConfirm">
                <div class="modal-content modal-content-danger">
                    <header class="modal-header modal-header-danger">
                        <h2>Confirmar eliminación</h2>
                        <button class="modal-close" type="button" @click="closeDeleteConfirm">&times;</button>
                    </header>
                    <div class="modal-body">
                        <p class="confirm-text">¿Estás seguro que deseas eliminar este ticket?</p>
                    </div>
                    <footer class="modal-footer">
                        <button class="modal-cancel-btn" type="button" @click="closeDeleteConfirm">Cancelar</button>
                        <button class="modal-submit-btn modal-submit-danger" type="button" @click="confirmDelete" :disabled="isDeleting">
                            <span v-if="!isDeleting">Confirmar</span>
                            <span v-else class="button-spinner"></span>
                        </button>
                    </footer>
                </div>
            </div>
        </transition>

        <transition name="modal-fade">
            <div v-if="showDetailModal" class="modal-overlay" @click.self="closeDetail">
                <div class="modal-content modal-detail">
                    <header class="modal-header">
                        <h2>Detalle del Ticket</h2>
                        <button class="modal-close" type="button" @click="closeDetail">&times;</button>
                    </header>
                    <div class="modal-body">
                        <div v-if="isLoadingDetail" class="loading-text">Cargando...</div>
                        <template v-else-if="ticketDetail">
                            <div class="detail-summary">
                                <div class="detail-item">
                                    <span class="detail-label">Fecha emisión</span>
                                    <span class="detail-value">{{ ticketDetail.fecha }}</span>
                                </div>
                                <div class="detail-item">
                                    <span class="detail-label">Total</span>
                                    <span class="detail-value expense">{{ formatCurrency(ticketDetail.total) }}</span>
                                </div>
                            </div>
                            <h3 class="products-title">Productos</h3>
                            <div v-if="!ticketDetail.productoRequests || ticketDetail.productoRequests.length === 0" class="empty-state">
                                Sin productos.
                            </div>
                            <ul v-else class="products-list">
                                <li v-for="(prod, idx) in ticketDetail.productoRequests" :key="idx" class="product-item">
                                    <span class="prod-name">{{ prod.descripcion }}</span>
                                    <span class="prod-qty">x{{ prod.cantidad }}</span>
                                    <span class="prod-price">{{ formatCurrency(prod.precio_unitario) }}</span>
                                </li>
                            </ul>
                        </template>
                    </div>
                </div>
            </div>
        </transition>

        <transition name="modal-fade">
            <div v-if="showCreateModal" class="modal-overlay" @click.self="closeCreateModal">
                <div class="modal-content modal-create">
                    <header class="modal-header">
                        <h2>Crear Ticket</h2>
                        <button class="modal-close" type="button" @click="closeCreateModal">&times;</button>
                    </header>
                    <div class="modal-body">
                        <table class="products-table">
                            <thead>
                                <tr>
                                    <th>Descripción</th>
                                    <th>Cantidad</th>
                                    <th>Precio Unit.</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="(row, idx) in productRows" :key="idx">
                                    <td><input v-model="row.descripcion" type="text" placeholder="Producto..." class="table-input" /></td>
                                    <td><input v-model.number="row.cantidad" type="number" min="1" class="table-input table-input-sm" /></td>
                                    <td><input v-model.number="row.precio_unitario" type="number" min="0" step="0.01" class="table-input table-input-sm" /></td>
                                    <td><button v-if="productRows.length > 1" class="remove-row-btn" type="button" @click="removeProductRow(idx)">&times;</button></td>
                                </tr>
                            </tbody>
                        </table>
                        <button class="add-row-btn" type="button" @click="addProductRow">+ Agregar producto</button>
                    </div>
                    <footer class="modal-footer">
                        <button class="modal-cancel-btn" type="button" @click="closeCreateModal">Cancelar</button>
                        <button class="modal-submit-btn" type="button" @click="handleCreateTicket" :disabled="isCreating">
                            <span v-if="!isCreating">Crear Ticket</span>
                            <span v-else class="button-spinner"></span>
                        </button>
                    </footer>
                </div>
            </div>
        </transition>

        <div class="tickets-header">
            <h1>Tickets</h1>
            <button class="create-btn" type="button" @click="openCreateModal">Crear Ticket</button>
        </div>

        <div class="tickets-content">
            <div v-if="isLoading" class="loading-text">Cargando tickets...</div>
            <div v-else-if="tickets.length === 0" class="empty-state">Sin tickets registrados</div>
            <ul v-else class="ticket-list">
                <li v-for="ticket in tickets" :key="ticket.ticket_id" class="ticket-item">
                    <span class="ticket-id">Ticket {{ ticket.ticket_id }}</span>
                    <span class="ticket-date">Fecha de Emision: {{ ticket.fcha_emision }}</span>
                    <span class="ticket-total">Total: {{ formatCurrency(ticket.cuenta_total) }}</span>
                    <div class="ticket-actions">
                        <button class="detail-btn" type="button" @click="openDetail(ticket.ticket_id)">Ver Detalle</button>
                        <button class="delete-btn" type="button" @click="openDeleteConfirm(ticket.ticket_id)">Eliminar</button>
                    </div>
                </li>
            </ul>
        </div>
    </div>
</template>

<style scoped>
.tickets-screen {
    width: 100%;
    min-height: calc(100vh - 60px);
    padding: 16px;
    position: relative;
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.tickets-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    background: rgba(15, 23, 42, 0.95);
    border: 1px solid rgba(44, 199, 124, 0.35);
    border-radius: 16px;
    padding: 16px 24px;
    color: #ffffff;
}

.tickets-header h1 {
    margin: 0;
    font-size: 22px;
    font-weight: 700;
}

.create-btn {
    padding: 10px 20px;
    border: none;
    border-radius: 8px;
    background: linear-gradient(135deg, var(--primary-color), #00d79a);
    color: #0f172a;
    font-weight: 700;
    font-size: 13px;
    cursor: pointer;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
}

.create-btn:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 12px rgba(44, 199, 124, 0.25);
}

.tickets-content {
    flex: 1;
    background: rgba(15, 23, 42, 0.95);
    border: 1px solid rgba(44, 199, 124, 0.35);
    border-radius: 16px;
    padding: 20px 24px;
    color: #ffffff;
    overflow-y: auto;
}

.loading-text {
    text-align: center;
    color: rgba(255, 255, 255, 0.5);
    padding: 24px;
    font-size: 13px;
}

.empty-state {
    text-align: center;
    color: rgba(255, 255, 255, 0.55);
    padding: 40px 8px;
    font-size: 14px;
}

.ticket-list {
    list-style: none;
    margin: 0;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.ticket-item {
    display: flex;
    align-items: center;
    gap: 16px;
    padding: 12px 14px;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
    border-radius: 10px;
    transition: background 0.15s ease;
}

.ticket-item:hover {
    background: rgba(255, 255, 255, 0.08);
}

.ticket-id {
    font-size: 14px;
    font-weight: 700;
    color: #ffffff;
    white-space: nowrap;
}

.ticket-date {
    font-size: 13px;
    color: rgba(255, 255, 255, 0.7);
    white-space: nowrap;
}

.ticket-total {
    font-size: 13px;
    font-weight: 700;
    color: #ff6b6b;
    white-space: nowrap;
    margin-left: auto;
}

.ticket-actions {
    display: flex;
    gap: 8px;
}

.detail-btn {
    padding: 5px 12px;
    border: none;
    border-radius: 6px;
    background: linear-gradient(135deg, #4dabf7, #228be6);
    color: #ffffff;
    font-size: 11px;
    font-weight: 600;
    cursor: pointer;
    transition: transform 0.15s ease;
}

.detail-btn:hover {
    transform: translateY(-1px);
}

.delete-btn {
    padding: 5px 12px;
    border: none;
    border-radius: 6px;
    background: #ff4d4d;
    color: #ffffff;
    font-size: 11px;
    font-weight: 600;
    cursor: pointer;
    transition: background 0.15s ease, transform 0.15s ease;
}

.delete-btn:hover {
    background: #e63939;
    transform: translateY(-1px);
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

.toast-error { background: #ff0000; border: 1px solid #c72828; color: #fff7f7; }
.toast-success { background: #00ff59; border: 1px solid #86efac; }

.toast-fade-enter-active, .toast-fade-leave-active { transition: opacity 0.2s ease, transform 0.2s ease; }
.toast-fade-enter-from, .toast-fade-leave-to { opacity: 0; transform: translateY(-8px); }

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

.modal-detail { width: min(90%, 620px); }
.modal-create { width: min(90%, 680px); max-height: 80vh; display: flex; flex-direction: column; }
.modal-create .modal-body { overflow-y: auto; flex: 1; min-height: 0; }

.modal-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
    padding-bottom: 10px;
    border-bottom: 1px solid rgba(77, 171, 247, 0.3);
}

.modal-header h2 { margin: 0; font-size: 20px; font-weight: 700; color: #4dabf7; }

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

.modal-close:hover { color: #ffffff; }

.modal-body { display: flex; flex-direction: column; gap: 12px; }

.modal-footer { display: flex; justify-content: flex-end; gap: 10px; margin-top: 16px; }

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

.modal-cancel-btn:hover { background: rgba(255, 255, 255, 0.08); }

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

.modal-submit-btn:disabled { opacity: 0.6; cursor: not-allowed; }
.modal-submit-btn:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 4px 12px rgba(77, 171, 247, 0.3); }

.modal-content-danger { border-color: rgba(255, 107, 107, 0.5); }
.modal-header-danger { border-bottom-color: rgba(255, 107, 107, 0.3); }
.modal-header-danger h2 { color: #ff6b6b; }
.modal-submit-danger { background: linear-gradient(135deg, #ff6b6b, #e03131); }
.modal-submit-danger:hover:not(:disabled) { box-shadow: 0 4px 12px rgba(255, 107, 107, 0.3); }

.confirm-text { margin: 0; font-size: 14px; color: rgba(255, 255, 255, 0.85); }

.detail-summary {
    display: flex;
    gap: 24px;
    padding: 12px 14px;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
    border-radius: 8px;
}

.detail-item { display: flex; flex-direction: column; gap: 2px; }
.detail-label { font-size: 13px; color: rgba(255, 255, 255, 0.6); }
.detail-value { font-size: 16px; font-weight: 700; color: #ffffff; }
.detail-value.expense { color: #ff6b6b; }

.products-title { margin: 14px 0 10px; font-size: 16px; font-weight: 700; color: var(--primary-color); }

.products-list {
    list-style: none;
    margin: 0;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: 6px;
    max-height: 240px;
    overflow-y: auto;
}

.product-item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 10px 12px;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
    border-radius: 6px;
    font-size: 14px;
}

.prod-name { flex: 1; color: #ffffff; font-weight: 500; }
.prod-qty { color: rgba(255, 255, 255, 0.7); font-weight: 600; }
.prod-price { color: #ff6b6b; font-weight: 700; }

.products-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 14px;
}

.products-table th {
    text-align: left;
    padding: 6px 8px;
    color: rgba(255, 255, 255, 0.7);
    font-weight: 600;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.products-table td { padding: 6px 8px; }

.table-input {
    width: 100%;
    padding: 9px 12px;
    border-radius: 6px;
    border: 1px solid rgba(77, 171, 247, 0.3);
    background: rgba(0, 0, 0, 0.4);
    color: #ffffff;
    font-size: 14px;
    outline: none;
    transition: border-color 0.2s ease;
}

.table-input:focus { border-color: #4dabf7; }
.table-input::placeholder { color: rgba(255, 255, 255, 0.35); }
.table-input-sm { width: 80px; }

.remove-row-btn {
    background: none;
    border: none;
    color: #ff6b6b;
    font-size: 18px;
    cursor: pointer;
    padding: 0 6px;
    line-height: 1;
    transition: color 0.15s ease;
}

.remove-row-btn:hover { color: #e03131; }

.add-row-btn {
    margin-top: 8px;
    padding: 8px 14px;
    border: 1px dashed rgba(44, 199, 124, 0.4);
    border-radius: 8px;
    background: transparent;
    color: var(--primary-color);
    font-size: 12px;
    font-weight: 600;
    cursor: pointer;
    transition: background 0.15s ease, border-color 0.15s ease;
    width: 100%;
}

.add-row-btn:hover {
    background: rgba(44, 199, 124, 0.08);
    border-color: var(--primary-color);
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

@keyframes spin { to { transform: rotate(360deg); } }

.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }

@media (max-width: 640px) {
    .ticket-item { flex-wrap: wrap; }
    .ticket-actions { width: 100%; justify-content: flex-end; }
    .table-input-sm { width: 60px; }
}
</style>
