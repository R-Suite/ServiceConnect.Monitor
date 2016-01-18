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

define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/serviceDetails.html',
    "backgrid",
    'app/collections/messageTypes',
    'toastr',
    'app/views/deleteServiceModal',
    'select2'
], function(Backbone, _, $, template, Backgrid, MessageTypeCollection, toastr, DeleteServiceModal) {

    "use strict";

    var view = Backbone.View.extend({

        events: {
            "click .saveServiceDetails": "_saveDetails",
            "click .deleteService": "_showDeleteModal"
        },

        columns: [{
            name: "Name",
            label: "",
            cell: "string",
            headerClassName: "hidden",
            editable: false
        }],

        bindings: {
            ".name": "Name",
            ".location": "InstanceLocation",
            ".consumerType": "ConsumerType",
            ".language": "Language"
        },

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);
            this.stickit();

            this.inMessagesCollection = new MessageTypeCollection();
            this.outMessagesCollection = new MessageTypeCollection();

            if (this.model.get("In") !== null && this.model.get("In") !== undefined) {
                var inMessages = this.model.get("In");
                for (var i = 0; i < inMessages.length; i++) {
                    this.inMessagesCollection.add(new Backbone.Model({
                        Name: inMessages[i]
                    }));
                }
            }
            if (this.model.get("Out") !== null && this.model.get("Out") !== undefined) {
                var outMessages = this.model.get("Out");
                for (var j = 0; j < outMessages.length; j++) {
                    this.outMessagesCollection.add(new Backbone.Model({
                        Name: outMessages[j]
                    }));
                }
            }

            this.inGrid = new Backgrid.Grid({
                columns: this.columns,
                collection: this.inMessagesCollection
            });
            this.$el.find(".inGrid").html(this.inGrid.render().$el);

            var inPaginator = new Backgrid.Extension.Paginator({
                collection: this.inMessagesCollection
            });
            this.$el.find(".inPaginator").html(inPaginator.render().$el);

            this.outGrid = new Backgrid.Grid({
                columns: this.columns,
                collection: this.outMessagesCollection
            });
            this.$el.find(".outGrid").html(this.outGrid.render().$el);

            var outPaginator = new Backgrid.Extension.Paginator({
                collection: this.outMessagesCollection
            });
            this.$el.find(".outPaginator").html(outPaginator.render().$el);

            this.$el.find(".backgrid").addClass("table-hover");
            this.$el.find(".backgrid").addClass("table-bordered");

            var that = this;
            this.$el.find(".tags").select2({
                multiple: true,
                createSearchChoice: function(term, data) {
                    if ($(data).filter(function() {
                            return this.text.localeCompare(term) === 0;
                        }).length === 0) {
                        return {
                            id: term,
                            text: term
                        };
                    }
                },
                allowClear: true,
                query: function(query) {
                    $.ajax({
                        url: "/tags",
                        data: {
                            query: query.term
                        },
                        success: function(data) {
                            var results = [];
                            for (var i = 0; i < data.length; i++) {
                                results.push({
                                    id: data[i],
                                    text: data[i]
                                });
                            }
                            query.callback({
                                results: results
                            });
                        }
                    });
                },
                initSelection: function(element, callback) {
                    if (!that.model.get("Tags")) {
                        that.model.set("Tags", []);
                        return;
                    }
                    var tags = that.model.get("Tags");
                    var data = [];
                    for (var i = 0; i < tags.length; i++) {
                        data.push({
                            id: tags[i],
                            text: tags[i]
                        });
                    }
                    callback(data);
                }
            }).on("select2-selecting", function(e, d) {
                that.model.get("Tags").push(e.object.id);
            }).on("select2-removed", function(e, d) {
                var index;
                $.each(that.model.get("Tags"), function(i, tag) {
                    if (tag === e.val) {
                        index = i;
                    }
                });
                that.model.get("Tags").splice(index, 1);
            }).select2('val', []);

            return this;
        },

        _saveDetails: function(e) {
            e.preventDefault();
            this.model.save(null, {
                success: function() {
                    toastr.success("Saved Service Tags");
                }
            });
        },

        _showDeleteModal: function(e) {
            e.preventDefault();
            var modal = new DeleteServiceModal({
                model: this.model
            });
            $("body").append(modal.render().$el);
        },

        onClose: function() {
            this.unstickit();
        }
    });

    return view;
});
