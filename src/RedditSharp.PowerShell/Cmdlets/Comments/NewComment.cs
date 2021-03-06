﻿using System;
using System.Management.Automation;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets.Comments
{
    /// <summary>
    /// <para type="synopsis">Create a new comment.</para>
    /// <para type="description">Create a new comment.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "Comment")]
    [OutputType(typeof(Comment))]
    public class NewComment : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Post or Comment to reply to.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByInputObject",Mandatory = true, Position = 0, ValueFromPipeline = true,
             HelpMessage = "Post or Comment to reply to.")]
        [ValidateNotNullOrEmpty]
        public VotableThing InputObject { get; set; }

        /// <summary>
        /// <para type="description">Replying to a comment or post.  This is probably going to go away</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByType",Mandatory = true,Position = 0,HelpMessage = "Replying to a Comment or a Post")]
        [ValidateSet(new []{"Comment","Post"})]
        public string TargetType { get; set; }

        /// <summary>
        /// <para type="description">Base35 id of the target.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByType", Mandatory = true, Position = 1,HelpMessage = "Base36 id of the Target")]
        [ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// <para type="description">Comment body.  Supports markdown.</para>
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Comment text/markdown")]
        [ValidateNotNullOrEmpty]
        public string Body { get; set; }

        protected override void BeginProcessing()
        {
            if (Session.Reddit == null)
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NoRedditSession",
                    ErrorCategory.InvalidOperation, Session.Reddit));
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "ByType")
            {
                if (Id.StartsWith("t1_") || Id.StartsWith("t3_"))
                    Id = Id.Remove(0, 3);
            }

            if (InputObject == null)
            {
                switch (TargetType)
                {
                    case "Post":
                        CommentPost(Id);
                        break;
                    case "Comment":
                        CommentComment(Id);
                        break;
                }
            }
            else
            {
                if (InputObject is Comment)
                    CommentComment(InputObject);
                else
                    CommentPost(InputObject);
            }
        }


        private void CommentPost(VotableThing target)
        {
            try
            {
                var p = target as Post;
                var c =p.Comment(Body);
                WriteObject(c);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CannotReplyToPost", ErrorCategory.InvalidOperation, InputObject));
            }
        }

        private void CommentPost(string postId)
        {
            try
            {
                // var json = "{ kind: 't3', data: { id : '" + postId + "', name: 't3_" + postId + "' } }";
                // var token = JToken.Parse(json);
                //  var p = new Post().Init(Session.Reddit, token, Session.WebAgent);
                var p = (Post)Helpers.GetSkeletonThing<Post>(postId);
                var c = p.Comment(Body);
                WriteObject(c);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CannotReplyToPost", ErrorCategory.InvalidOperation, InputObject));
            }
        }

        private void CommentComment(VotableThing target) // bork bork
        {
            try
            {
                var c = target as Comment;
                c.Reply(Body);
                WriteObject(c);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CannotReplyToComment", ErrorCategory.InvalidOperation, InputObject));
            }
        }

        private void CommentComment(string commentId)
        {
            try
            {
               // var json = "{ kind: 't1', data: { id: '" + commentId + "', name: 't1_" + commentId + "'} }";
               // var token = JToken.Parse(json);

               // var c = new Comment().Init(Session.Reddit, token, Session.WebAgent,null);

                var c = (Comment)Helpers.GetSkeletonThing<Comment>(commentId);
                c.Reply(Body);
                WriteObject(c);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CannotReplyToComment", ErrorCategory.InvalidOperation, InputObject));
            }
        }
    }
}
