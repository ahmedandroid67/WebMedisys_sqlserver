// theme.js - Add this to your wwwroot/js folder

(function () {
    'use strict';

    // Theme management
    const ThemeManager = {
        // Get theme from localStorage or system preference
        getTheme: function () {
            const savedTheme = localStorage.getItem('theme');
            if (savedTheme) {
                return savedTheme;
            }

            // Check system preference
            if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                return 'dark';
            }

            return 'light';
        },

        // Set theme
        setTheme: function (theme) {
            document.documentElement.setAttribute('data-theme', theme);
            localStorage.setItem('theme', theme);
            this.updateToggleButton(theme);
        },

        // Toggle between light and dark
        toggleTheme: function () {
            const currentTheme = this.getTheme();
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            this.setTheme(newTheme);
        },

        // Update toggle button icon and text
        updateToggleButton: function (theme) {
            const toggleBtn = document.getElementById('theme-toggle');
            if (!toggleBtn) return;

            const icon = toggleBtn.querySelector('i');
            const text = toggleBtn.querySelector('.theme-text');

            if (theme === 'dark') {
                icon.className = 'fa-solid fa-sun';
                if (text) text.textContent = 'Light';
            } else {
                icon.className = 'fa-solid fa-moon';
                if (text) text.textContent = 'Dark';
            }
        },

        // Initialize theme
        init: function () {
            const theme = this.getTheme();
            this.setTheme(theme);

            // Add event listener to toggle button
            const toggleBtn = document.getElementById('theme-toggle');
            if (toggleBtn) {
                toggleBtn.addEventListener('click', () => this.toggleTheme());
            }

            // Listen for system theme changes
            if (window.matchMedia) {
                window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                    if (!localStorage.getItem('theme')) {
                        this.setTheme(e.matches ? 'dark' : 'light');
                    }
                });
            }
        }
    };

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => ThemeManager.init());
    } else {
        ThemeManager.init();
    }

    // Make ThemeManager available globally if needed
    window.ThemeManager = ThemeManager;
})();