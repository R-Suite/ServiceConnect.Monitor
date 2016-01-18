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
    'bower_components/requirejs-text/text!app/templates/deleteServiceModal.html'
], function(Backbone, _, $, template) {

    "use strict";

    var view = Backbone.View.extend({

        events: {
            "click .yesDeleteModal": "_delete",
            "click .noDeleteModal": "_closeModal"
        },

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);
            this.$el.find(".deleteServiceModal").modal('show');
            this.$el.find(".deleteServiceModal").on('hidden.bs.modal', this.closeView);
            return this;
        },

        _delete: function() {
            var that = this;
            this.model.destroy({
                success: function() {
                    that.$el.find(".deleteServiceModal").off('hidden.bs.modal', that.closeView);
                    that.$el.find(".deleteServiceModal").on('hidden.bs.modal', that._deleted);
                    that.$el.find(".deleteServiceModal").modal('hide');
                }
            });
        },

        _closeModal: function() {
            this.$el.find(".deleteServiceModal").modal('hide');
        },

        _deleted: function() {
            window.location = "#";
            this.$el.find(".deleteServiceModal").off('hidden.bs.modal', this._deleted);
            this.close();
        },

        closeView: function() {
            this.close();
        },

        onClose: function() {
            this.$el.find(".deleteServiceModal").off('hidden.bs.modal', this.closeView);
            this.remove();
        }
    });

    return view;
});
