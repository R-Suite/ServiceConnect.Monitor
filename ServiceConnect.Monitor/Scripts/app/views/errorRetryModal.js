//Copyright (C) 2015  Timothy Watson, Jakub Pachansky

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License, or (at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

define([
    'backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/errorRetryModal.html',
    "toastr"
], function(Backbone, _, $, template, toastr) {

    "use strict";

    var view = Backbone.View.extend({

        events: {
            "click .closeModal": "_closeModal",
            "click .confirm": "_retry"
        },

        initialize: function(options) {
            this.onComplete = options.onComplete;
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);
            this.$el.find(".errorRetryModal").modal('show');
            this.$el.find(".errorRetryModal").on('hidden.bs.modal', this.closeView);
            return this;
        },

        _closeModal: function() {
            this.$el.find(".errorRetryModal").modal('hide');
        },

        closeView: function() {
            this.close();
        },

        _retry: function() {
            var that = this;

            var callbacks = {
                success: function() {
                    toastr.success("Successfully retried messages");
                    that._closeModal();
                    that.onComplete();
                },
                error: function() {
                    toastr.success("Error retrying messages");
                    that._closeModal();
                }
            };

            if (this.collection) {
                this.collection.retry(callbacks);
            } else {
                this.model.retry(callbacks);
            }
        },

        onClose: function() {
            this.$el.find(".errorRetryModal").off('hidden.bs.modal', this.closeView);
            this.remove();
        }
    });

    return view;
});
