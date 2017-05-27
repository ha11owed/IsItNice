function insert(item, user, request) {
    item.CreatedAt = new Date();
    
    var channelTable = tables.getTable('Channel');
    channelTable.where({ UserId: user.userId }).read({
        success: function(users) {
            if(users.length === 1) {
                item.FromId = users[0].id;
                request.execute({
                    success: function() {
                        request.respond();
                        sendNotifications();
                    }
                });
            }
            else {
                request.respond(statusCodes.INTERNAL_SERVER_ERROR, 'duplicate channel');
            }
        }
    });
    
    function sendNotifications() {
        var channelTable = tables.getTable('Channel');
        // Only select the channels of the request owner
        channelTable.where({ id: item.RequestOwnerId }).read({
            success: function(channels) {
                channels.forEach(function(channel) {
                    push.wns.sendRaw(channel.uri, JSON.stringify(item));
                });
            }
        });
    }
}
