// === equipment.js ===
// Управление панелью оборудования (финальная версия с device/full API)

document.addEventListener("DOMContentLoaded", () => {
    const sidebar = document.querySelector(".equipment-sidebar");
    const modesContainer = document.querySelector(".equipment-modes");
    const controlContainer = document.querySelector(".equipment-control");

    window.currentDevice = null;

    sidebar.innerHTML = "<p class='placeholder'>Загрузка устройств...</p>";

    // === 1. Загрузка списка устройств ===
    fetch("/api/equipment/devices")
        .then(r => {
            if (!r.ok) throw new Error(`Ошибка HTTP ${r.status}`);
            return r.json();
        })
        .then(({ categories, devices }) => {
            sidebar.innerHTML = "<h3>Оборудование</h3>";

            categories.forEach(cat => {
                const div = document.createElement("div");
                div.className = "device-category";
                div.innerHTML = `<h4>${cat.category}</h4>`;

                const items = devices.filter(d => d.category === cat.category);
                if (items.length > 0) {
                    div.innerHTML += items.map(i =>
                        `<button class="device-btn"
                            data-device="${i.key}"
                            data-interface="${i.interfaceType}">
                            ${i.name}
                        </button>`
                    ).join("");
                } else {
                    div.innerHTML += `<p class="placeholder" style="font-size:13px;">Нет устройств</p>`;
                }

                sidebar.appendChild(div);
            });

            document.querySelectorAll(".device-btn").forEach(btn => {
                btn.addEventListener("click", () => {
                    document.querySelectorAll(".device-btn").forEach(b => b.classList.remove("active"));
                    btn.classList.add("active");

                    const deviceKey = btn.dataset.device;
                    loadDeviceAndModes(deviceKey);
                });
            });
        })
        .catch(err => {
            console.error("Ошибка при загрузке устройств:", err);
            sidebar.innerHTML = "<p class='placeholder'>Ошибка загрузки данных</p>";
        });

    // === 2. Загрузка устройства + режимов ===
    function loadDeviceAndModes(deviceKey) {
        modesContainer.innerHTML = "<p class='placeholder'>Загрузка режимов...</p>";
        controlContainer.innerHTML = "<p class='placeholder'>Загрузка данных об устройстве...</p>";

        fetch(`/api/equipment/device/full/${deviceKey}`)
            .then(r => {
                if (!r.ok) throw new Error(`Ошибка HTTP ${r.status}`);
                return r.json();
            })
            .then(result => {
                const { device, modes } = result;
                window.currentDevice = device;

                renderDeviceInfo(device.entity);
                renderModes(modes, device);
            })
            .catch(err => {
                console.error("Ошибка при загрузке устройства:", err);
                controlContainer.innerHTML = "<p class='placeholder'>Ошибка загрузки устройства</p>";
                modesContainer.innerHTML = "<p class='placeholder'>Ошибка загрузки режимов</p>";
            });
    }

    // === 3. Карточка устройства ===
    function renderDeviceInfo(device) {
        controlContainer.innerHTML = `
            <div class="mode-panel">
                <h4>${device.Name ?? device.name ?? "Неизвестное устройство"}</h4>
                <p><b>ID:</b> ${device.Id ?? device.id ?? "—"}</p>
                <p><b>Описание:</b> ${device.Description ?? device.description ?? "—"}</p>
                <p><b>Подключение:</b> ${device.ConnectionDetails ?? device.connectionDetails ?? "—"}</p>
                <p style="opacity:.6;margin-top:8px;">Выберите режим для управления устройством.</p>
            </div>
        `;
    }

    // === 4. Рендер режимов ===
    function renderModes(modes, device) {
        if (!modes || modes.length === 0) {
            modesContainer.innerHTML = "<p class='placeholder'>Режимы не найдены</p>";
            return;
        }

        modesContainer.innerHTML = modes.map(m =>
            `<button class="mode-btn" data-mode="${m.name}">${m.name}</button>`
        ).join("");

        document.querySelectorAll(".mode-btn").forEach(mb => {
            mb.addEventListener("click", () => loadControlPanel(device, mb.dataset.mode));
        });
    }

    // === 5. Панель управления ===
    function loadControlPanel(device, modeName) {
        controlContainer.innerHTML = "<p class='placeholder'>Загрузка панели управления...</p>";

        const category = device.__category || "unknown";

        fetch(`/api/equipment/control/${encodeURIComponent(category)}/${encodeURIComponent(modeName)}`)
            .then(r => {
                if (!r.ok) throw new Error(`Ошибка HTTP ${r.status}`);
                return r.json();
            })
            .then(data => {
                controlContainer.innerHTML = `
                    <div class="mode-panel">
                        <h4>${data.title}</h4>
                        <p>${data.description}</p>
                        <div class="btn-group">
                            ${data.buttons.map(b => `<button class="btn-blue">${b}</button>`).join("")}
                        </div>
                    </div>`;
            })
            .catch(err => {
                console.error("Ошибка при загрузке панели управления:", err);
                controlContainer.innerHTML = "<p class='placeholder'>Ошибка загрузки панели</p>";
            });
    }
});
