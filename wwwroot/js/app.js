const state = {
  users: [],
  currentUser: null,
  userItems: [],
  adminItems: [],
  deleteId: null
};

const priorityNames = ["Low", "Medium", "High"];
const priorityClasses = ["bg-secondary", "bg-warning text-dark", "bg-danger"];

const elements = {
  userSelect: document.querySelector("#userSelect"),
  roleBadge: document.querySelector("#roleBadge"),
  pageTitle: document.querySelector("#pageTitle"),
  loadingView: document.querySelector("#loadingView"),
  userView: document.querySelector("#userView"),
  adminView: document.querySelector("#adminView"),
  userItemsBody: document.querySelector("#userItemsBody"),
  adminItemsBody: document.querySelector("#adminItemsBody"),
  userStatusText: document.querySelector("#userStatusText"),
  confirmButton: document.querySelector("#confirmButton"),
  workItemForm: document.querySelector("#workItemForm"),
  formTitle: document.querySelector("#formTitle"),
  saveButton: document.querySelector("#saveButton"),
  cancelEditButton: document.querySelector("#cancelEditButton")
};

const detailModal = bootstrap.Modal.getOrCreateInstance(document.querySelector("#detailModal"));
const deleteModal = bootstrap.Modal.getOrCreateInstance(document.querySelector("#deleteModal"));

async function apiRequest(url, options = {}) {
  const config = {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {})
    }
  };

  const response = await fetch(url, config);
  if (!response.ok) {
    let message = `操作失敗 (${response.status})`;
    try {
      const body = await response.json();
      message = body.error?.message || body.title || message;
    } catch (_) {}
    throw new Error(message);
  }

  if (response.status === 204) return null;
  const text = await response.text();
  return text ? JSON.parse(text) : null;
}

function showToast(message, type = "success") {
  const toastElement = document.querySelector("#appToast");
  toastElement.className = `toast border-0 text-bg-${type === "success" ? "success" : "danger"}`;
  document.querySelector("#toastMessage").textContent = message;
  bootstrap.Toast.getOrCreateInstance(toastElement, { delay: 2600 }).show();
}

function escapeHtml(value) {
  if (value === null || value === undefined) return "";
  const node = document.createElement("span");
  node.textContent = String(value);
  return node.innerHTML;
}

function formatDate(value) {
  if (!value) return "—";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "—";
  return date.toLocaleDateString("zh-TW", { year: "numeric", month: "2-digit", day: "2-digit" });
}

function dateOnly(value) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

async function loadUsers() {
  state.users = await apiRequest("/api/auth/users");
  elements.userSelect.innerHTML = state.users.map(user =>
    `<option value="${user.id}" data-login="${escapeHtml(user.loginName)}">${escapeHtml(user.userName)} (${escapeHtml(user.loginName)})</option>`
  ).join("");

  const savedId = localStorage.getItem("workItemUserId");
  const savedUser = state.users.find(user => user.id === savedId) || state.users.find(user => user.loginName === "alice") || state.users[0];
  if (savedUser) {
    await switchUser(savedUser.id, false);
  }
}

async function switchUser(userId, persist = true) {
  const user = state.users.find(x => x.id === userId);
  if (!user) return;

  const loggedIn = await apiRequest("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ loginName: user.loginName })
  });

  state.currentUser = loggedIn;
  elements.userSelect.value = loggedIn.id;
  if (persist) localStorage.setItem("workItemUserId", loggedIn.id);

  elements.loadingView.classList.remove("d-none");
  elements.userView.classList.add("d-none");
  elements.adminView.classList.add("d-none");

  if (loggedIn.role === 1) {
    elements.roleBadge.textContent = "管理者";
    elements.roleBadge.className = "badge rounded-pill role-badge admin";
    elements.pageTitle.textContent = "後台管理";
    await loadAdminItems();
  } else {
    elements.roleBadge.textContent = "一般使用者";
    elements.roleBadge.className = "badge rounded-pill role-badge";
    elements.pageTitle.textContent = "前台工作清單";
    await loadUserItems();
  }

  elements.loadingView.classList.add("d-none");
}

async function loadUserItems() {
  state.userItems = await apiRequest(`/api/users/${state.currentUser.id}/work-items`);
  renderUserItems();
  elements.userView.classList.remove("d-none");
}

function renderUserItems() {
  if (!state.userItems.length) {
    elements.userItemsBody.innerHTML = `<tr><td colspan="6" class="empty-state">目前沒有 Work Item</td></tr>`;
    elements.confirmButton.disabled = true;
    elements.userStatusText.textContent = "共 0 筆項目";
    return;
  }

  elements.confirmButton.disabled = false;
  const confirmedCount = state.userItems.filter(item => item.isConfirmed).length;
  elements.userStatusText.textContent = `共 ${state.userItems.length} 筆；已確認 ${confirmedCount} 筆`;

  elements.userItemsBody.innerHTML = state.userItems.map(item => {
    const statusBadge = item.isConfirmed
      ? `<span class="badge rounded-pill confirmed-badge">已確認</span>`
      : `<span class="badge rounded-pill pending-badge">待確認</span>`;

    return `<tr>
      <td class="text-center">
        <input class="form-check-input work-check" type="checkbox" data-id="${item.id}" ${item.isChecked ? "checked" : ""} aria-label="選擇 ${escapeHtml(item.title)}">
      </td>
      <td><button class="item-title-button" type="button" data-detail-id="${item.id}">${escapeHtml(item.title)}</button></td>
      <td>${escapeHtml(item.category || "—")}</td>
      <td><span class="badge priority-badge ${priorityClasses[item.priority]}">${priorityNames[item.priority]}</span></td>
      <td>${formatDate(item.dueDate)}</td>
      <td>${statusBadge}</td>
    </tr>`;
  }).join("");
}

async function confirmStatuses() {
  const statuses = [...document.querySelectorAll(".work-check")].map(check => ({
    workItemId: check.dataset.id,
    isChecked: check.checked
  }));

  elements.confirmButton.disabled = true;
  elements.confirmButton.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span>儲存中...`;

  try {
    const result = await apiRequest(`/api/users/${state.currentUser.id}/work-item-statuses/confirm`, {
      method: "POST",
      body: JSON.stringify({ statuses })
    });

    await loadUserItems();
    showToast(`儲存成功！已確認 ${result.confirmedCount} 筆項目`, "success");
  } catch (error) {
    showToast(error.message, "danger");
  } finally {
    elements.confirmButton.disabled = false;
    elements.confirmButton.innerHTML = `<i class="bi bi-cloud-check me-1"></i>Confirm 儲存狀態`;
  }
}

async function showDetail(itemId) {
  const item = await apiRequest(`/api/work-items/${itemId}`);
  const personal = state.userItems.find(x => x.id === itemId);

  document.querySelector("#detailTitle").textContent = item.title;
  document.querySelector("#detailBody").innerHTML = `
    <dl class="row mb-0">
      <dt class="col-4">標題</dt><dd class="col-8">${escapeHtml(item.title)}</dd>
      <dt class="col-4">詳細資料</dt><dd class="col-8">${escapeHtml(item.description || "無")}</dd>
      <dt class="col-4">類別</dt><dd class="col-8">${escapeHtml(item.category || "—")}</dd>
      <dt class="col-4">優先度</dt><dd class="col-8">${priorityNames[item.priority]}</dd>
      <dt class="col-4">到期日</dt><dd class="col-8">${formatDate(item.dueDate)}</dd>
      ${personal ? `<dt class="col-4">勾選狀態</dt><dd class="col-8">${personal.isChecked ? "已勾選" : "未勾選"}</dd>
      <dt class="col-4">確認狀態</dt><dd class="col-8">${personal.isConfirmed ? `已確認（${formatDate(personal.confirmedAt)}）` : "尚未確認"}</dd>` : ""}
    </dl>`;
  detailModal.show();
}

async function loadAdminItems() {
  state.adminItems = await apiRequest("/api/work-items");
  renderAdminItems();
  elements.adminView.classList.remove("d-none");
}

function renderAdminItems() {
  if (!state.adminItems.length) {
    elements.adminItemsBody.innerHTML = `<tr><td colspan="5" class="empty-state">目前沒有 Work Item</td></tr>`;
    return;
  }

  elements.adminItemsBody.innerHTML = state.adminItems.map(item => `<tr>
    <td><button class="item-title-button" type="button" data-admin-detail-id="${item.id}">${escapeHtml(item.title)}</button></td>
    <td>${escapeHtml(item.category || "—")}</td>
    <td><span class="badge priority-badge ${priorityClasses[item.priority]}">${priorityNames[item.priority]}</span></td>
    <td>${formatDate(item.dueDate)}</td>
    <td class="text-end">
      <button class="btn btn-sm btn-outline-primary me-1" type="button" data-edit-id="${item.id}" title="編輯"><i class="bi bi-pencil"></i></button>
      <button class="btn btn-sm btn-outline-danger" type="button" data-delete-id="${item.id}" title="刪除"><i class="bi bi-trash"></i></button>
    </td>
  </tr>`).join("");
}

function resetForm() {
  elements.workItemForm.reset();
  elements.workItemForm.classList.remove("was-validated");
  document.querySelector("#itemId").value = "";
  document.querySelector("#priority").value = "1";
  elements.formTitle.textContent = "新增 Work Item";
  elements.saveButton.innerHTML = `<i class="bi bi-check-lg me-1"></i>儲存`;
  elements.cancelEditButton.classList.add("d-none");
}

function editItem(itemId) {
  const item = state.adminItems.find(x => x.id === itemId);
  if (!item) return;
  document.querySelector("#itemId").value = item.id;
  document.querySelector("#title").value = item.title;
  document.querySelector("#description").value = item.description || "";
  document.querySelector("#category").value = item.category || "";
  document.querySelector("#priority").value = String(item.priority);
  document.querySelector("#dueDate").value = dateOnly(item.dueDate);
  elements.formTitle.textContent = "編輯 Work Item";
  elements.saveButton.innerHTML = `<i class="bi bi-save me-1"></i>更新`;
  elements.cancelEditButton.classList.remove("d-none");
  document.querySelector("#workItemForm").scrollIntoView({ behavior: "smooth", block: "start" });
}

async function saveWorkItem(event) {
  event.preventDefault();
  const form = elements.workItemForm;
  form.classList.add("was-validated");
  if (!form.checkValidity()) return;

  const itemId = document.querySelector("#itemId").value;
  const dueDateValue = document.querySelector("#dueDate").value;
  const payload = {
    title: document.querySelector("#title").value.trim(),
    description: document.querySelector("#description").value.trim() || null,
    category: document.querySelector("#category").value.trim() || null,
    priority: Number(document.querySelector("#priority").value),
    dueDate: dueDateValue ? `${dueDateValue}T00:00:00+08:00` : null
  };

  elements.saveButton.disabled = true;
  try {
    if (itemId) {
      await apiRequest(`/api/work-items/${itemId}`, {
        method: "PUT",
        headers: { "X-User-Id": state.currentUser.id },
        body: JSON.stringify(payload)
      });
      showToast("Work Item 已更新", "success");
    } else {
      await apiRequest("/api/work-items", {
        method: "POST",
        headers: { "X-User-Id": state.currentUser.id },
        body: JSON.stringify(payload)
      });
      showToast("Work Item 已新增", "success");
    }
    resetForm();
    await loadAdminItems();
  } catch (error) {
    showToast(error.message, "danger");
  } finally {
    elements.saveButton.disabled = false;
  }
}

async function deleteWorkItem() {
  if (!state.deleteId) return;
  const button = document.querySelector("#confirmDeleteButton");
  button.disabled = true;
  try {
    await apiRequest(`/api/work-items/${state.deleteId}`, {
      method: "DELETE",
      headers: { "X-User-Id": state.currentUser.id }
    });
    deleteModal.hide();
    resetForm();
    await loadAdminItems();
    showToast("Work Item 已刪除", "success");
  } catch (error) {
    showToast(error.message, "danger");
  } finally {
    button.disabled = false;
  }
}

function openDeleteModal(itemId) {
  const item = state.adminItems.find(x => x.id === itemId);
  if (!item) return;
  state.deleteId = itemId;
  document.querySelector("#deleteItemTitle").textContent = item.title;
  deleteModal.show();
}

elements.userSelect.addEventListener("change", async event => {
  try {
    await switchUser(event.target.value, true);
  } catch (error) {
    showToast(error.message, "danger");
  }
});

elements.confirmButton.addEventListener("click", confirmStatuses);
elements.workItemForm.addEventListener("submit", saveWorkItem);
elements.cancelEditButton.addEventListener("click", resetForm);
document.querySelector("#refreshAdminButton").addEventListener("click", loadAdminItems);
document.querySelector("#confirmDeleteButton").addEventListener("click", deleteWorkItem);

document.addEventListener("change", event => {
  if (!event.target.matches(".work-check")) return;
  const item = state.userItems.find(x => x.id === event.target.dataset.id);
  if (item) {
    item.isChecked = event.target.checked;
    item.isConfirmed = false;
  }
  renderUserItems();
  const restoredCheck = document.querySelector(`.work-check[data-id="${event.target.dataset.id}"]`);
  if (restoredCheck) restoredCheck.checked = event.target.checked;
});

document.addEventListener("click", event => {
  const detailButton = event.target.closest("[data-detail-id]");
  if (detailButton) showDetail(detailButton.dataset.detailId).catch(error => showToast(error.message, "danger"));

  const adminDetailButton = event.target.closest("[data-admin-detail-id]");
  if (adminDetailButton) {
    apiRequest(`/api/work-items/${adminDetailButton.dataset.adminDetailId}`).then(item => {
      document.querySelector("#detailTitle").textContent = item.title;
      document.querySelector("#detailBody").innerHTML = `
        <dl class="row mb-0">
          <dt class="col-4">標題</dt><dd class="col-8">${escapeHtml(item.title)}</dd>
          <dt class="col-4">詳細資料</dt><dd class="col-8">${escapeHtml(item.description || "無")}</dd>
          <dt class="col-4">類別</dt><dd class="col-8">${escapeHtml(item.category || "—")}</dd>
          <dt class="col-4">優先度</dt><dd class="col-8">${priorityNames[item.priority]}</dd>
          <dt class="col-4">到期日</dt><dd class="col-8">${formatDate(item.dueDate)}</dd>
        </dl>`;
      detailModal.show();
    }).catch(error => showToast(error.message, "danger"));
  }

  const editButton = event.target.closest("[data-edit-id]");
  if (editButton) editItem(editButton.dataset.editId);

  const deleteButton = event.target.closest("[data-delete-id]");
  if (deleteButton) openDeleteModal(deleteButton.dataset.deleteId);
});

loadUsers().catch(error => {
  elements.loadingView.innerHTML = `<div class="alert alert-danger d-inline-block">無法載入身分資料：${escapeHtml(error.message)}</div>`;
});
