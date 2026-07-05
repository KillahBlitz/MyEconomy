const baseUrl = import.meta.env.VITE_API_URL;


export async function loginUser(user, password) {
  const response = await fetch(`${baseUrl}login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ usuario: user, password }),
  });
  return response.json();
}

export async function getCategories() {
    const response = await fetch(`${baseUrl}Categories`, {
      method: 'GET'
    });

    return response.json();
}

export async function getDashBoardContability(params) {
    const cachedUserId = localStorage.getItem('userId');
    const userId = cachedUserId ? parseInt(cachedUserId, 10) : 0;

    try {
        const response = await fetch(`${baseUrl}GetDashboardContability`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ 
                userId: userId,
                startDate: params.startDate,
                endDate: params.endDate,
                neededSaldo: params.neededSaldo,
                categories: params.categories
            }),      
        });
        const data = await response.json();
        return data;

    } catch (error) {
        throw error;
    }
}

export async function createSaldo({ concepto, cantidad, id_usuario, id_categoria }) {
    const response = await fetch(`${baseUrl}Saldo`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            saldo_id: 0,
            concepto,
            cantidad,
            id_usuario,
            id_categoria
        }),
    });
    return response.json();
}

export async function deleteSaldo(id) {
    const response = await fetch(`${baseUrl}Saldo/${id}`, {
        method: 'DELETE',
    });
    return response.json();
}