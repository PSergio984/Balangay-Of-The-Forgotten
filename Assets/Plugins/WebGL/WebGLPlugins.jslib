mergeInto(LibraryManager.library, {
  // Shows a confirmation prompt before reloading the page. If bypassPrompt is true, reloads immediately.
  RefreshPage: function (bypassPrompt) {
    if (typeof bypassPrompt === "undefined" || !bypassPrompt) {
      var confirmed = window.confirm(
        "Are you sure you want to refresh? Unsaved progress may be lost."
      );
      if (confirmed) {
        location.reload();
      }
    } else {
      location.reload();
    }
  },
});
