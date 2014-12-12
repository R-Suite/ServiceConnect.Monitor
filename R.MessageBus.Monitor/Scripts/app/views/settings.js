define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/settings.html',
    "backgrid",
    'toastr'
], function(Backbone, _, $, template, Backgrid, toastr) {

    "use strict";

    var view = Backbone.View.extend({

        el: ".mainContent",

        events: {
            "click .saveSettings": "_saveSettings",
            "click .addEnvironment": "_addEnvironment",
            "click .removeEnvironment": "_removeEnvironment"
        },

        columns: [{
            name: "Server",
            cell: "string"
        }, {
            name: "Username",
            cell: "string"
        }, {
            name: "Password",
            cell: "string"
        }, {
            name: "ErrorQueue",
            cell: "string"
        }, {
            name: "AuditQueue",
            cell: "string"
        }, {
            name: "HeartbeatQueue",
            cell: "string"
        }, {
            label: "",
            name: 'id',
            cell: 'html',
            editable: false,
            formatter: _.extend({}, Backgrid.CellFormatter.prototype, {
                fromRaw: function(rawValue, model) {
                    return "<a href='javascript:void(0)' class='removeEnvironment' cid='" + model.cid + "' >Remove</a>";
                }
            })
        }],

        bindings: {
            ".ForwardErrors": {
                observe: 'ForwardErrors',
                onGet: function(value) {
                    if (value) {
                        $(".ForwardErrorQueueContainer").show();
                    } else {
                        $(".ForwardErrorQueueContainer").hide();
                    }
                    return value;
                },
                onSet: function(value) {
                    if (value) {
                        $(".ForwardErrorQueueContainer").show();
                    } else {
                        $(".ForwardErrorQueueContainer").hide();
                    }
                    return value;
                }
            },
            ".ForwardErrorQueue": "ForwardErrorQueue",
            ".ForwardAudit": {
                observe: 'ForwardAudit',
                onGet: function(value) {
                    if (value) {
                        $(".ForwardAuditQueueContainer").show();
                    } else {
                        $(".ForwardAuditQueueContainer").hide();
                    }
                    return value;
                },
                onSet: function(value) {
                    if (value) {
                        $(".ForwardAuditQueueContainer").show();
                    } else {
                        $(".ForwardAuditQueueContainer").hide();
                    }
                    return value;
                }
            },
            ".ForwardAuditQueue": "ForwardAuditQueue",
            ".ForwardHeartbeats": {
                observe: 'ForwardHeartbeats',
                onGet: function(value) {
                    if (value) {
                        $(".ForwardHeartbeatQueueContainer").show();
                    } else {
                        $(".ForwardHeartbeatQueueContainer").hide();
                    }
                    return value;
                },
                onSet: function(value) {
                    if (value) {
                        $(".ForwardHeartbeatQueueContainer").show();
                    } else {
                        $(".ForwardHeartbeatQueueContainer").hide();
                    }
                    return value;
                }
            },
            ".KeepErrorsFor": "KeepErrorsFor",
            ".KeepAuditsFor": "KeepAuditsFor",
            ".KeepHeartbeatsFor": "KeepHeartbeatsFor"
        },

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);

            this.grid = new Backgrid.Grid({
                columns: this.columns,
                collection: this.model.get("Environments")
            });
            this.$el.find(".grid").html(this.grid.render().$el);

            this.$el.find(".backgrid").addClass("table-hover");
            this.$el.find(".backgrid").addClass("table-bordered");

            this.stickit();

            return this;
        },

        _saveSettings: function(e) {
            e.preventDefault();
            this.model.save(null, {
                success: function() {
                    toastr.success("Saved settings");
                },
                errors: function() {
                    toastr.success("Failed to save settings");
                }
            });
        },

        _addEnvironment: function(e) {
            e.preventDefault();
            this.model.get("Environments").add(new Backbone.Model({
                ErrorQueue: "errors",
                AuditQueue: "audit",
                HeartbeatQueue: "heartbeat",
                Username: "guest",
                Password: "guest"
            }));
        },

        _removeEnvironment: function(e) {
            e.preventDefault();
            var cid = $(e.currentTarget).attr("cid");
            this.model.get("Environments").remove(cid);
        },

        onClose: function() {
            this.unstickit();
        }
    });

    return view;
});
