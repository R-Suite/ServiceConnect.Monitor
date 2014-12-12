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
