const createPawsDialog = () => {
  let activeResolve = null;
  let dialogQueue = Promise.resolve();
  const backdrop = document.createElement("div");
  backdrop.className = "paws-dialog-backdrop";
  backdrop.innerHTML = `
    <div class="paws-dialog" role="dialog" aria-modal="true" aria-labelledby="pawsDialogTitle">
      <div class="paws-dialog-icon" aria-hidden="true"></div>
      <div class="paws-dialog-body">
        <div class="page-kicker" id="pawsDialogKicker">PAWS Notice</div>
        <h2 id="pawsDialogTitle"></h2>
        <p id="pawsDialogMessage"></p>
      </div>
      <div class="paws-dialog-actions">
        <button type="button" class="btn btn-outline-secondary" data-paws-cancel>Cancel</button>
        <button type="button" class="btn btn-primary" data-paws-confirm>OK</button>
      </div>
    </div>`;
  document.body.appendChild(backdrop);

  const dialog = backdrop.querySelector(".paws-dialog");
  const title = backdrop.querySelector("#pawsDialogTitle");
  const message = backdrop.querySelector("#pawsDialogMessage");
  const kicker = backdrop.querySelector("#pawsDialogKicker");
  const confirmButton = backdrop.querySelector("[data-paws-confirm]");
  const cancelButton = backdrop.querySelector("[data-paws-cancel]");

  const close = (result) => {
    backdrop.classList.remove("is-visible");
    document.body.classList.remove("paws-dialog-open");
    const resolve = activeResolve;
    activeResolve = null;
    resolve?.(result);
  };

  confirmButton.addEventListener("click", () => close(true));
  cancelButton.addEventListener("click", () => close(false));
  backdrop.addEventListener("click", (event) => {
    if (event.target === backdrop) {
      close(false);
    }
  });
  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && backdrop.classList.contains("is-visible")) {
      close(false);
    }
  });

  const open = (options) => new Promise((resolve) => {
    activeResolve = resolve;
    title.textContent = options.title || "Notice";
    message.textContent = options.message || "";
    kicker.textContent = options.kicker || "PAWS Notice";
    confirmButton.textContent = options.confirmText || "OK";
    cancelButton.textContent = options.cancelText || "Cancel";
    cancelButton.hidden = !options.showCancel;
    dialog.className = `paws-dialog paws-dialog-${options.type || "info"}`;
    document.body.classList.add("paws-dialog-open");
    backdrop.classList.add("is-visible");
    confirmButton.focus();
  });

  const enqueue = (options) => {
    const nextDialog = dialogQueue.then(() => open(options), () => open(options));
    dialogQueue = nextDialog.catch(() => {});
    return nextDialog;
  };

  return {
    alert: (options) => enqueue({ ...options, showCancel: false }),
    confirm: (options) => enqueue({ ...options, showCancel: true })
  };
};

window.PawsDialog = window.PawsDialog || createPawsDialog();

document.addEventListener("DOMContentLoaded", () => {
  const header = document.querySelector(".app-header");
  const nav = document.getElementById("mainNavigation");

  const updateHeader = () => {
    header?.classList.toggle("is-scrolled", window.scrollY > 8);
  };

  updateHeader();
  window.addEventListener("scroll", updateHeader, { passive: true });

  nav?.querySelectorAll("a.nav-link:not(.dropdown-toggle), .dropdown-item").forEach((link) => {
    link.addEventListener("click", () => {
      if (window.innerWidth >= 1200 || !nav.classList.contains("show") || !window.bootstrap) {
        return;
      }

      window.bootstrap.Collapse.getOrCreateInstance(nav).hide();
    });
  });

  document.querySelectorAll("form[data-paws-confirm]").forEach((form) => {
    form.addEventListener("submit", async (event) => {
      if (form.dataset.pawsConfirmed === "true") {
        return;
      }

      event.preventDefault();
      const confirmed = await window.PawsDialog.confirm({
        title: "Confirm Delete",
        message: form.dataset.pawsConfirm,
        type: "danger",
        confirmText: "Delete",
        cancelText: "Cancel"
      });

      if (confirmed) {
        form.dataset.pawsConfirmed = "true";
        form.submit();
      }
    });
  });

  document.querySelectorAll(".alert.alert-danger, .alert.alert-success").forEach((alert) => {
    const message = alert.textContent.trim();
    if (!message) {
      return;
    }

    window.PawsDialog.alert({
      title: alert.classList.contains("alert-success") ? "Success" : "Action Needed",
      message,
      type: alert.classList.contains("alert-success") ? "success" : "danger"
    });
  });

  document.querySelectorAll(".validation-summary-errors").forEach((summary) => {
    const message = Array.from(summary.querySelectorAll("li"))
      .map((item) => item.textContent.trim())
      .filter(Boolean)
      .join("\n");

    if (message) {
      window.PawsDialog.alert({
        title: "Please Check the Form",
        message,
        type: "warning"
      });
    }
  });

  const attachTableFilter = (input, tableShell) => {
    const rows = Array.from(tableShell?.querySelectorAll("tbody tr") || [])
      .filter((row) => !row.querySelector(".empty-state"));

    if (rows.length === 0) {
      return;
    }

    input.setAttribute("autocomplete", "off");
    input.addEventListener("input", () => {
      const term = input.value.trim().toLowerCase();
      rows.forEach((row) => {
        row.hidden = term.length > 0 && !row.textContent.toLowerCase().includes(term);
      });
    });
  };

  document.querySelectorAll(".search-panel input[name='searchTerm']").forEach((input) => {
    attachTableFilter(input, input.closest(".search-panel")?.nextElementSibling);
  });

  document.querySelectorAll(".table-shell").forEach((tableShell) => {
    if (tableShell.previousElementSibling?.querySelector?.("input[name='searchTerm']")) {
      return;
    }

    const rows = Array.from(tableShell.querySelectorAll("tbody tr"))
      .filter((row) => !row.querySelector(".empty-state"));
    if (rows.length === 0) {
      return;
    }

    const tools = document.createElement("div");
    tools.className = "table-tools";
    tools.innerHTML = '<input type="search" class="form-control" placeholder="Quick filter this table" autocomplete="off" />';
    tableShell.parentNode.insertBefore(tools, tableShell);
    attachTableFilter(tools.querySelector("input"), tableShell);
  });

  document.querySelectorAll(".table-shell table").forEach((table) => {
    table.querySelectorAll("thead th").forEach((header, columnIndex) => {
      if (header.classList.contains("text-end")) {
        return;
      }

      header.classList.add("sortable-header");
      header.setAttribute("role", "button");
      header.setAttribute("tabindex", "0");

      const sortRows = () => {
        const tbody = table.querySelector("tbody");
        const rows = Array.from(tbody?.querySelectorAll("tr") || [])
          .filter((row) => !row.querySelector(".empty-state"));
        const direction = header.dataset.sortDirection === "asc" ? "desc" : "asc";

        table.querySelectorAll("thead th").forEach((item) => {
          item.removeAttribute("data-sort-direction");
        });
        header.dataset.sortDirection = direction;

        rows.sort((left, right) => {
          const leftValue = left.children[columnIndex]?.textContent.trim().toLowerCase() || "";
          const rightValue = right.children[columnIndex]?.textContent.trim().toLowerCase() || "";
          return direction === "asc"
            ? leftValue.localeCompare(rightValue, undefined, { numeric: true })
            : rightValue.localeCompare(leftValue, undefined, { numeric: true });
        });

        rows.forEach((row) => tbody.appendChild(row));
      };

      header.addEventListener("click", sortRows);
      header.addEventListener("keydown", (event) => {
        if (event.key === "Enter" || event.key === " ") {
          event.preventDefault();
          sortRows();
        }
      });
    });
  });

  const appointmentStatusSelect = document.getElementById("Status");
  const cancellationSection = document.getElementById("cancellationSection");

  if (appointmentStatusSelect && cancellationSection) {
    const syncCancellationSection = () => {
      cancellationSection.hidden = appointmentStatusSelect.value !== "Cancelled";
    };

    appointmentStatusSelect.addEventListener("change", syncCancellationSection);
    syncCancellationSection();
  }

  const staffAppointmentForm = document.querySelector("[data-staff-appointment-form]");
  if (staffAppointmentForm) {
    const existingPatientToggle = staffAppointmentForm.querySelector("#IsExistingPatient");
    const patientNumberSection = staffAppointmentForm.querySelector("[data-staff-patient-number-section]");
    const newPatientNote = staffAppointmentForm.querySelector("[data-staff-new-patient-note]");
    const patientNumberInput = staffAppointmentForm.querySelector("#PatientNoInput");
    const petDetailsSection = staffAppointmentForm.querySelector("[data-staff-pet-details-section]");
    const petDetailFields = staffAppointmentForm.querySelectorAll("[data-staff-pet-detail-field]");

    const syncStaffAppointmentPatientMode = () => {
      const isExistingPatient = existingPatientToggle?.checked === true;

      if (patientNumberSection) {
        patientNumberSection.hidden = !isExistingPatient;
      }

      if (newPatientNote) {
        newPatientNote.hidden = isExistingPatient;
      }

      if (patientNumberInput) {
        patientNumberInput.disabled = !isExistingPatient;
        patientNumberInput.classList.toggle("patient-number-disabled", !isExistingPatient || patientNumberInput.readOnly);
        if (!isExistingPatient && !patientNumberInput.readOnly) {
          patientNumberInput.value = "";
        }
      }

      if (petDetailsSection) {
        petDetailsSection.hidden = isExistingPatient;
      }

      petDetailFields.forEach((field) => {
        field.disabled = isExistingPatient;
      });
    };

    existingPatientToggle?.addEventListener("change", syncStaffAppointmentPatientMode);
    syncStaffAppointmentPatientMode();
  }
});
