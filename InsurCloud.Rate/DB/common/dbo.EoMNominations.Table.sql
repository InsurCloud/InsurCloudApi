USE [Common]
GO
/****** Object:  Table [dbo].[EoMNominations]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EoMNominations](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Location] [varchar](50) NOT NULL,
	[Month] [varchar](50) NOT NULL,
	[Nominator] [varchar](50) NOT NULL,
	[Reason] [varchar](2000) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_EoMNominations] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET IDENTITY_INSERT [dbo].[EoMNominations] ON 

INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (2, N'Maria Brito', N'Florida', N'201310', N'IMPERIAL\isabel.andres', N'Very hard worker, dependable, efficient, knowledgeable, always willing to help another coworker with any question/problem he/she may have.

Very professional and very good attitude when providing Customer Service in any area..', CAST(0x0000A25A00619745 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (3, N'Esther Marcos ', N'Florida', N'201310', N'IMPERIAL\maria.brito', N'Esther is one of our customer service representative. She treats all customers and fellow employees with respect. She goes behond her duties to help fellow employees. When she is asked for a favor she is always willing to help. ', CAST(0x0000A25A006A9E80 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (4, N'JULIE BAKER', N'Texas', N'201310', N'IMPERIAL\lrodriguez', N'JULIE ALWAYS HAS A SMILE ON HER FACE AND WILL GO OVER AND BEYOND FOR IMPERIAL.  SHE IS A TEAM PLAYER AND WILL HELP IN EVERY WAY POSSIBLE. BESIDES HANDLING ACCOUNTING AND HR, JULIE ALWAYS ASSITS THE UNDERWRITTING DEPARTMENT TO MAKE SURE OUR POLICIES ARE TAKEN CARE OF. ', CAST(0x0000A25A00A09D39 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (5, N'Kay Fontenot', N'Louisiana', N'201310', N'IMPERIAL\chassidy.Ford', N'Always helping others, always smiling,and is very good with customers and agents on the phones. anytime you have a question she helps you without a problem if she can.', CAST(0x0000A25A00A0C5D1 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (6, N'Lynn Burkhalter', N'Louisiana', N'201310', N'IMPERIAL\michelle.bourque', N'She is always there for the company. Whenever an adjuster or clerical needs her; she gives her full attention and helps as much as she can. I believe she deserves being the Employee of the Month. She represents Imperial very well. She speaks very well to the customers and makes them feel that she is giving them their full attention. ', CAST(0x0000A25A00A1C31C AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (7, N'ALICIA VIDAL', N'Florida', N'201310', N'IMPERIAL\teresa.barro', N'THIS IS A PERSON THAT ALWAY HELP EVERYBODY, SHE''S VERY NIECE , VERY PROFESSIONAL AND ALWAYS HAVE SMILE IN HER FACE .. AND SHE''S ALWAYS COMPLY WITH ALL THE INSTRUCTIONS 
SHE WORK IN THE PIP DEPARTMENT , SHE''S IN TOP OF ALL THE AVTIVITY IN THE COMPANY , AS FRIEND FOR ME SHE''S THE BEST. I CONSIDER THAT SHE CAN BE THE EMPLOYEE OF THE MONTH.', CAST(0x0000A25A00A455C7 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (8, N'Julie Baker', N'Texas', N'201310', N'IMPERIAL\posborn', N'Julie has gone above and beyond her office duties to help out others.  When our dept was short handed, she stepped up and helped us to keep current.   When asked to look at an issue with us, no problems.  The issue gets resolved.
She definately exemplifies the GARD Values of this company.', CAST(0x0000A25A00A4E6D9 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (9, N'Tim Maher', N'Texas', N'201310', N'IMPERIAL\ethan.porter', N'I believe Tim has shown exactly what the GARD values truly mean. In every interaction I hear him in it is truly evident that he is being 100% fair and in good faith. He accessbile in the fact that he answers every question I have no matter how busy he is. He is extremely resourceful and knowledgable in the industry. I have learned many tricks from him. He is extremely dependable in the fact that he is here at 7:30 every morning and stays till 7:30 at night and even sometimes later than that. I believe 100% that he should be the employee of the month.', CAST(0x0000A25A00A5F67D AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (10, N'Lynn Burkhalter', N'Louisiana', N'201310', N'IMPERIAL\christine.hawkins', N'The reason why I’m nominating this person is because I think she represents the values of the Company; she is a person that will take the roll of friend/mother/counselor/guide/teacher or what ever is needed at the time.

This person is fair in her dealings with co-workers and customers, she is accessible, we can approach her anytime and the way I see and hear she will always answer her calls, if she does not know an answer for a question she will research and if she tells you she will do something, you can be sure she will

I have seen this person come in the office early, leave late, come in sick a lot of times.

I have seen this person, providing great service to customers even though they might be rude, screaming on the phone, etc, She will always talk to them in an even tone and patiently calm them down. She has a tough job, and I really think she deserves the nomination and more!!!!
', CAST(0x0000A25A00A68428 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (11, N'Joseph Daugereaux', N'Louisiana', N'201310', N'IMPERIAL\boydg', N'He is always so helpful whenever asked and has a very positive attitude



He is always accessible and dependable and always resourceful in getting the task at hand done. 



I have seen Joseph walking through our department on more than one occasion asking if any of us were having any problems or needing anything. This is over and above the call of duty.


He does everything with a smile. Never a grumble about anything. 
', CAST(0x0000A25A00A72B26 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (12, N'Michelle Bourque', N'Louisiana', N'201310', N'IMPERIAL\sharon.cox', N'She always willing to help others
Everyday she has a smile on her face and very up beat and positive. She exemplify the GARD Values 
always on time and willing to stay late when needed.', CAST(0x0000A25A00AE4AEA AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (14, N'Maria Brito', N'Florida', N'201310', N'IMPERIAL\amalia.castro', N'She is very puntual, very professional with insured''s clmt''s and attorney''s, very knowledgeable in her field, good customer service skills, and allways willing to help others,', CAST(0x0000A25A00EC2362 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (15, N'Bryan Barringer', N'Texas', N'201310', N'IMPERIAL\Cassandra.Cline-Gera', N'Bryan consistently works with me on projects to make sure the data I need is correct and working the way I need it to.  He stays on top of things, and never lets a request fall through the cracks.  He doesn''t hesitate to think outside the box, and is able to come up with a solution that works.  He''s a pleasure to work with.', CAST(0x0000A25A00F7BC38 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (16, N'Osaris Fonseca', N'Florida', N'201310', N'IMPERIAL\marlene.fernandez', N'Osaris works the suspense as well as phone calls in our underwriting dept , she keeps all our daily suspense up to date, along with answering phones from agents and insureds, always treating everyone with with respect and making sure they are given all the correct information on all pending information needed to complete our applications, she is also very self efficient when working with agent and insures handling all calls with courtecy but firm on all our underwriting quidelines  Osaris is very efficent and always goes over and beyond her job duties .        ', CAST(0x0000A25A00FC55E6 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (17, N'Martha Sanchez', N'Florida', N'201310', N'IMPERIAL\michelle.leal', N'This person never says no when asked for assistance.  Martha is always available to hear you and if unable to help at that moment she will take the assignment and will complete it in a reasonable amount of time.  She goes above and beyond her scope.  Martha keeps an assignment until the job gets done and does not pass it off to someone else.  

I go to her often because her experience and connections in this business are valuable to us when we need information that is accurate and timely.

Her customer service skills are reflected by the fact that attorneys and their staff respond well to her and usually assist her in whatever requests she makes.', CAST(0x0000A25E008B8350 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (18, N'Lynn Burkhalter', N'Louisiana', N'201310', N'IMPERIAL\Tammy.Ducote', N'Lynn follows the GARD values each and every day.  She has been with the company for many, many years and the 2 years I have been with Imperial, she has fair dealings with each and every employee as well as with the insureds & claimants I have heard her speak to. She never answers irate people loudly, she is a very dependable person, i.e. helps co-workers with questions, is always around when I need her.  She has handled all LA adjusters'' licensing matters even when had illness in her family.  She is just an all-around great person and great co-worker and employee to Imperial.', CAST(0x0000A25F00B2B401 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (19, N'Julie Baker', N'Texas', N'201310', N'IMPERIAL\pzimmerman', N'GARD Values - She is loyal, honest, dependable, respectful, kind and thoughtful.
Over and Beyond - She is one person that handles several different jobs. (Accounting and HR)
Customer Service - She responds to any phone request or email promptly and with professionalism.', CAST(0x0000A25F00BA92FD AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (20, N'MARTHA SANCHEZ', N'Florida', N'201310', N'IMPERIAL\yasmin.mendoza', N'IN MY OPINION YOU SHOULD CONSIDER THIS PERSON BECAUSE SHE GOES OUT OF HER WAY TO ACCOMPLISH HER RESPONSABILITIES, ALSO SHE PROVIDES GREAT CUSTOMER SERVICE AND GOES BEYOND HER CALL OF DUTY. ', CAST(0x0000A26000EEE1E0 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (21, N'Faye Gautro', N'Louisiana', N'201310', N'IMPERIAL\darla.pitre', N'Faye comes in with a smile every day and is always willing to help others in anyway.  She is always friendly and always positive. Faye is a pleasure to work with and brings out the best in others.  Faye is a true example of what a real team player is.  

 ', CAST(0x0000A26000EEF2C7 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (22, N'Stephanie Buchanan', N'Texas', N'201310', N'IMPERIAL\Jamye.Westbrook', N'Stephanie is very helpful and extremely organized. She has helped out the IT department with getting organized tremendously! She has jumped in and helped out in many different situations.  She is always very pleasant and very professional to work with. Stephanie has been a great addition to the IT team and I believe she is a great examples of our core values. ', CAST(0x0000A26000F0F2A6 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (23, N'Ves Garrett', N'Louisiana', N'201310', N'IMPERIAL\scottp', N'I nominate Ves Garrett because of an issue he assisted me with the other day.  Ves demonstrated that he was "Accessable" in that he dropped what he was doing to help me figure out my issue (an Excel matter).  He was "Resourceful" in that he used the tools at his command to research the matter for me and found the solution.  Finally, he was "Dependable" as always.

Ves will always try to accommodate you when the time arises.  He is knowledgeable and he does not know the answer he will either find it or help you find it.

This is quite typical of Ves'' daily routine.

Thank you for considering him as EOM.', CAST(0x0000A26000F1923A AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (24, N'LISA LOWE', N'Florida', N'201310', N'IMPERIAL\andres.lopez', N'SHE’S A HARD WORKING PERSON, UNDERSTANDING AND GOES OVER THE CALL OF DUTY.
SHE TAKES THE TIME TO LISTEN AND FINDS SOLUTIONS TO VARIOUS SITUATIONS RELATED TO OUR EVERY DAY WORK. SHE GOES OUT OF HER WAY TO TRY TO HELP OTHERS WITHOUT REGARDS TO WHAT SHE’S DOING AT THAT MOMENT. SHE’S THERE WHEN YOU NEED HER. 
GOOD CO-WORKER.', CAST(0x0000A26000F4B4C2 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (25, N'Michelle Bourque', N'Louisiana', N'201310', N'IMPERIAL\Billy.Durel', N'Since starting with IFAC, Michelle has made herself available to me, in the same way she has made herself available to others.  She was a great resource for me when I was learning the "IFAC Way" and has not stopped.  It is one thing to say that she has made herself available to assist me in my handling of claims, but it is much more than that.  She has been pro-active in helping with challenges in handing of my claims.  While I speak from personal experience in dealing with Michelle, I''ve also witnessed her dedication to teamwork in dealing with other within the Claims Department.
Michelle treats our external customers with the same courtesy and dedication as her internal customers in Claims.  When she does not have the answer to questions the customer is asking she will make sure to get them to the correct person. It is easy to say "that is her job" but she does it with compassion and empathy, which is easy to lose sight of in this industry and her roll. 
 ', CAST(0x0000A26000F7063D AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (26, N'Maria Eusse', N'Florida', N'201310', N'IMPERIAL\yan.sanchez', N'Maria is an excellent employee and is always up to the task at hand. 

she always excels at her job duties and treats everyone with respect and places concern with each claim. 

She is always willing to answer any phone call that comes in and when she is scheduled to stay during lunches she does not complain when she is left alone to receive calls. ', CAST(0x0000A26000F901B7 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (27, N'Faye Goutro', N'Louisiana', N'201310', N'IMPERIAL\tamarica.trent', N'She is very punctually, always willing to go out of her way to get her job done and to help in any way she can. She''s always willing to learn something new, Mrs. Faye always have a smile on her face and great everyone she enconter with her welcomeing personallity.', CAST(0x0000A2600108EFE5 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (28, N'Jimmy Beaird', N'Texas', N'201310', N'IMPERIAL\Pamela.Banks', N'Has been training me > And has been absolutely FABULOUS! Has shown Great patients with me. . He is always upbeat and so willing to assist. He takes his time to make sure that your getting a clear understanding. I think he is a great asset to the company.', CAST(0x0000A260010AF912 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (29, N'Guillermo Callejas', N'Florida', N'201310', N'IMPERIAL\lisa.lowe', N'Guillermo Callejas always has a smile on his face and is willing to do anything to help out.  It seems that no matter what needs to be done if you give it to Guillermo you know he will get the job done in a proficient manner.  He might not be known by everyone in the office as he is not a social butterfly but I think that is what makes him a great employee.  He works hard and steps up for any challenge that is given to him.  His dedication and hard work help the PIP unit run as a well oiled machine.  ', CAST(0x0000A26100695CD1 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (30, N'Maria Brito', N'Florida', N'201310', N'IMPERIAL\martha.sanchez', N'Maria Brito is one of the best employee of Imperial in the Florida offices, her punctuality over the years, accurate handle of claims''s file, diary, negotiations, until she bring the claim file to a final conclusion.
She also has a great actitud toward fellow workers and
customers, she is over and Beyond the call of duty.
I highly recomend her for employee of the month. ', CAST(0x0000A261007B2CD0 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (31, N'Jayme Westbrooke ', N'Texas', N'201310', N'IMPERIAL\Jimmy.Beaird', N'this person always gets the job done very quickly and no matter how much worked this person has she is always very kind and always has a smile on her face.  Very hard worker and desrves to be recognized for her effort. Jayme provides excellent custer service by always taking care of computer issues and then following up the make sure the isssue has not returned and every thing is still working properly. Thank you. :-) ', CAST(0x0000A2610087FF5E AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (32, N'Martha Sanchez ', N'Florida', N'201310', N'IMPERIAL\maribel.humaran', N'Martha always brings to work her best attitude and work ethics. She is a very knowledgeable and experienced claims adjuster. She is a mentor to in our office. Martha is an example of what Imperial Fire and Casualty is all about.

', CAST(0x0000A26100FC86C8 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (33, N'Sue Willis', N'Louisiana', N'201310', N'IMPERIAL\phyllisc', N'Sue Willis is a dedicated long time employee of IFAC.  She has been asked on numerous occasions to perform work outside of her job duties/in addition to her job duties.  I can always depend on her to perform the task accurately and even provide additional information that may not be required but she believes may be helpful.  She is a huge resource for our claims department and has been resourceful in understanding what is needed for claims reports.  She makes it a point to make herself accessible to others.  She always has the best interest of IFAC in mind.  ', CAST(0x0000A26101281362 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (34, N'Kenny Mathis', N'Texas', N'201310', N'IMPERIAL\Stan.Hvostik', N'Kenny is very good at resolving our personal computer issues, including hardware and software.


Kenny is accessible, resourceful, and dependable.  When he solved my VPN issues, he gave me his cell phone number and told me to call after hours if I needed help.  Kenny was resourceful and dependable in getting MapPoint software installed, even though he ran into several obstacles.

Kenny provides excellent service and assistance in a cheerful manner, even when he has a heavy workload.



Kenny provides great customer service by living the GARD values in a cheerful way.
', CAST(0x0000A26200C37587 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (35, N'Darla Pitre', N'Louisiana', N'201310', N'IMPERIAL\fayeg', N'I AM NOMINATING DARLA PITRE, BECAUSE SHE FOLLOWS IMPERIAL
GUARD VALUES. SHE WORKS IN CUSTOMER SERVICE AND IS ALWAYS ON THE PHONE, SHE WILL STOP AND TRY TO HELP A PERSON EVEN IF
THEY ARE NOT IN UNDERWRITING. IT MAY BE IN ANOTHER DEPARTMENT, IF SHE CAN''T HELP THEM, SHE WILL GET SOME ONE ELSE TO. SHE WILL ASK THE PARTY QUESTIONS, TO SEE IF MAYBE SHE CAN HELP THEM. SHE IS AN EXCELLENT CHOICE FOR  A CUSTOMER SERIVCE REPRESENTATIVE.', CAST(0x0000A26200EC4794 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (36, N'Carmen Lambert-Vazquez', N'Florida', N'201310', N'IMPERIAL\alicia.vidal', N'I am nominating Carmen Lambert-Vazquez as in my opinion she is a great employee and very knowledgeable, especially managing the PIP Dept.  Carmen is always willing and ready to answer any question you may have and to guide you in the right direction.  She not only answers a Claim question but knows how to deal with any aspect of the PIP laws and will guide the adjusters in avoiding demands and lawsuits.
She is always ready to tackle on any problem or project with a great effort and professionalism.   

Carmen is an asset to our organization.  She treats everyone fairly and with respect.  She knows how to manage very tough situations which at times can be very challenging.  I have many times transferred calls for various reasons and she has always treated the caller with outmost respect demonstrating her professionalism.  Carmen is a leader in every sense of the word.   I am proud to work for such a fine person.   Carmen is a person that can analyze difficult situations and is able to have solutions without overstepping into someone’s boundaries.   

Carmen also possesses great communication skills.   I believe this is part of the reason why she has been
so successful in her career.

 
', CAST(0x0000A26200EC53DD AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (37, N'Janice Cordero', N'Florida', N'201310', N'IMPERIAL\iliana.sanchez', N'Im nominating Janice because I belive she gives by far the best customer service, she is always willing to help out anyone on the phone even when close to her lunchtime. She always is very professional and courteous to others. 

Janice exemplifies the GARd Values because she always treats everyone the same and cares about each claim, she is beyond well knowledgeable in what she does and is very dependable always on time and just an all around good person to be around and work with, 

I Believe Janice goes above and beyond when dealing with each claim she takes time to remember and learn each claim so she is more ready whenever the claim comes up and she is faced with a caller. 


She provides great CS because she is polite never loses her cool and treats everyone the same way. Shes always ready to answer and call that comes in for her.
', CAST(0x0000A26200EE111F AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (38, N'GERMAINE DAIGLE.', N'Louisiana', N'201310', N'IMPERIAL\Juan.Enriquez', N'THIS PERSON SHOULD BE CONSIDER EMPLOYEE OF THE MONTH BECAUSE OF HER GREAT WORK. SHE IS ALWAYS CHALLAGING HERSELF TO DO BETTER AND TO HELP OTHER ACHIEVE THEIR GOAL. SHE NOT ONLY EXEMPLIFY THE IMPERIAL GARD VALUES BUT PUT THEM TO THE TEST IN EVERYDAY''S WORK DAY.', CAST(0x0000A26200EE45F3 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (39, N'Melinda Dehoyos', N'Texas', N'201310', N'IMPERIAL\JoAnna.Harris', N'She accepted the challenge in taking the lead for our 5 new employees. Doing so, she maintained the demand of her own work load, providing superior service.I feel Melinda displays every aspect of our GARD values and even more so given this challenge. ', CAST(0x0000A26200EE74CE AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (40, N'Melissa Dupre', N'Louisiana', N'201310', N'IMPERIAL\stacey.morgan', N'Melissa is always upbeat and has a great attitude.  She''s always quick to offer assistance and if she doesn''t have an answer for you, she''ll find one.  She dependable and kind.  When I think of what Imperial stands for, I think of her.', CAST(0x0000A26200EEE6EF AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (41, N'Chassidy Ford ', N'Louisiana', N'201310', N'IMPERIAL\zoilap', N' good attitude with customers and co-workers, good customers service, good way to solve problems no matter how big or small the problem,good attitude, always smiling 
always helping others any questions you have the answer  with good attitude, ', CAST(0x0000A26200EF68A5 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (42, N'Roniel Thomas', N'Louisiana', N'201310', N'IMPERIAL\kasey.gennuso', N'Roniel is a great asset to Imperial. Ahe takes pride in her work and is always willing to help. She have great customer service and a great attitude!Anytime I call claims she is always willing to use a helping hand.', CAST(0x0000A26200F0BD50 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (43, N'Maria Brito', N'Florida', N'201310', N'IMPERIAL\luisa.perez', N'she is an awesome and effective worker 
.












































































', CAST(0x0000A26200FB6708 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (44, N'Stacey Morgan', N'Texas', N'201310', N'IMPERIAL\Blanca.Carrillo', N'Stacey Morgan is always friendly and willing to help. She has a hard working and positive disposition that embodies what the Imperial culture strives to be.', CAST(0x0000A26201006EFA AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (45, N'JOSE HERNANDEZ', N'Florida', N'201310', N'IMPERIAL\maria.perez', N'HE IS A VERY DEDICATED EMPLOYEE.  HE IS ALWAYS WILLING TO ANSWER ANY QUESTION AND AVAILABLE TO TAKE HIS PHONE CALLS. HE TRIES HARD TO KEEP UP WITH THE FLOW OF WORK AND PROCESS CLAIMS IN A TIMELY MANNER.  HE IS POLITE WITH CUSTOMERS ON THE PHONE.  ', CAST(0x0000A26201023201 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (46, N'Kay Fontenot', N'Louisiana', N'201310', N'IMPERIAL\melanieb', N'Kay is a pleasure to work with, she is always willing to stop what she is doing to help anyone. If Kay is on the phone with an irate customer she will do all she can to satisfy the customer , she will put the customer on hold and call their agent for them to see if they have faxed the information we need to lift the cancellation or she will waive a late fee(one time) ', CAST(0x0000A26201036FB5 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (47, N'Guillermo Callejas', N'Florida', N'201311', N'IMPERIAL\Carmen.Lambert', N'Guillermo is a team player and always readily available to assist whenever the need arises. I am constantly calling upon him to set aside his routine workload to assist me in urgent matters that involve meeting deadlines, identifying mathematical errors and preparing demand responses.  

Guillermo works very well with his co-workers within the unit as well as with other departments. He is respectful and kind. He is dependable and reliable. 


Guillermo comes to work early and stays late in order to meet our deadlines. Guillermo has demonstrated his loyalty and excellent work ethic in all matters presented to him. Specifically, over the last couple of weeks Guillermo has helped with the training of two new employees. I have had the pleasure of monitoring the training and am grateful that Guillermo is on my team. He is an asset to this organization. 

I have asked Guillermo to return phone calls for customers that are calling on closed files or files that do not have an adjuster assigned to him and Guillermo is intuitive and understands the callers concerns and needs. Together we work to find the solution and Guillermo makes every effort to satisfy the caller. For these reasons I would like to nominate Guillermo Callejas for employee of the month. 
', CAST(0x0000A2650089A892 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (48, N'Jayme Westbrook', N'Texas', N'201311', N'IMPERIAL\jbaker', N'Jayme always has a smile on her face no matter the time of the day or whatever task she is doing or how busy she might be. She goes beyond what is ''required'' to do something to be sure it''s done and works at 100% or more. I had issues with some of my software and she stayed at my desk until the issue was resolved instead of pulling the "I''ll be back later" that has happened in the past. She always finishes what she starts and makes sure it works, even following up days later to verify that it''s still working. I think she shows great examples of many of our GARD Values', CAST(0x0000A26D00F26573 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (49, N'Jamye Westbrook', N'Texas', N'201311', N'IMPERIAL\Andrea.Ward', N'Jamye is an IT Rock Star.  I recently added a new member to my underwriting team, and I was incredibly stressed about the process of getting him set up.  I''m fairly new to Imperial myself, and I had a very rough first few weeks when I started.  How I wish Jamye had been here back then! Jamye was on top of EVERYTHING: from the access badge, to setting up his workstation, to mapping hard drives, to email accounts...it was such a smooth process.  First impressions are key for new employees, and Jamye puts Imperial in the best possible light.  She is fast, professional, personable...she always has a smile and doesn''t grumble when I make a request or ask a dumb question. 

Internal customer service is often overlooked.  The service that Jayme provides to her co-workers goes above and beyond the call of duty.  ', CAST(0x0000A27500AD4276 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (50, N'Jamye Westbrook ', N'Texas', N'201311', N'IMPERIAL\Jimmy.Beaird', N'Jayme always handles herself very professionally and makes sure everything on your PC is working properly before she leaves your desk.  I know she is just as busy with numerous responsibilities as everyone esle here. If she is passing by and there is a quick question about an issue that needs to be solved, she always answers your questions with a smile.  You never feel like you have imposed on her day.  :-) ', CAST(0x0000A27C00A0C05D AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (51, N'Fernando Rodriguez ', N'Florida', N'201311', N'IMPERIAL\maria.brito', N'Fernando is a part time employee and goes beyond his duties to help anyone that needs it. Any time he is asked to do something he always does it with a good attitude. ', CAST(0x0000A27C00A29AA9 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (52, N'Nancy Thibodeaux', N'Louisiana', N'201311', N'IMPERIAL\chassidy.Ford', N'She is always very helpful no matter what when you call her about an accounting matter. Always has a smile on her face no matter when you see her. Does things promptly when e-mail is sent to her for something from me.', CAST(0x0000A27C00A2F336 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (53, N'Jamye Westbrook', N'Texas', N'201311', N'IMPERIAL\michael.wodek', N'Jamye Westbrook was brought in as a contractor this summer to provide support to the Infrastructure Team for our migration from Microsoft Exchange 2003 to Microsoft Exchange 2010. She was informed that this would be a project with no expectation for this to evolve into anything further relative to contract extension.

Jamye took on the role with this understanding and demonstrated outstanding Customer interface skills, a broad technical acumen, and a willingness to contribute in any area she could. This effort and energy allowed for her to take on Helpdesk and Customer support functions while her contract was extended.

Today Jamye is a full-time employee, a position she earned solely on her merits. We created the position in anticipation of future growth a month or so early so we would not lose the opportunity to bring her on board.

Since becoming full time Jamye has continued to provide an outstanding level of Customer service, a commitment to the success of the team and department, she will work whatever extra hours necessary to keep things moving forward, and demonstrated an uncanny ability to quickly learn new things as she grows into her position. Jamye has an energy level that is unrivaled. Her efforts kept us from falling behind in the Dallas office with PC deployments and overall on-site Customer support while we worked to replace another Helpdesk support team member who chose a different career path.

Jamye is very deserving of consideration for this award.

Regards,

Mike', CAST(0x0000A27C00A4654E AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (54, N'Tim Maher', N'Texas', N'201311', N'IMPERIAL\ethan.porter', N'Tim consistently goes "Over and Beyond the Call of Duty". He is constantly helping us Level 1 adjusters on top getting his inventory down. I see him stay late most days and seems to never let anything get him upset or down. I can''t think of a more deserving employee of the month!!', CAST(0x0000A27C00A49FDC AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (55, N'Pedro Caballero - BI Adjuster', N'Florida', N'201311', N'IMPERIAL\martha.sanchez', N'GOOD NEGOTIATIONS SKILL, GOOD BI SETTLEMENTS, PUNCTUALITY,
VERY PROFESSIONAL IN THE WAY HE HANDLE CLAIMS SITUATIONS,
GO OVER AND BEYOND THE CALL OF DUTY.

', CAST(0x0000A27C00A7BB37 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (56, N'Jamye Westbrook', N'Texas', N'201311', N'IMPERIAL\Blanca.Carrillo', N'Jamye is always dependable and brings a positive attitude to the office everyday. She works very hard, is always willing to help and accomplishes her tasks with a friendly disposition.', CAST(0x0000A27C00AA4270 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (57, N'Shawn Hargrove ', N'Texas', N'201311', N'IMPERIAL\nick.bogdan', N'sHawn is the Level 1 Claim Manager in the Texas office. He has been at Imperial for approximately 4 months and during this time he has been responsible for designing, developing and implementing an industry leading physical damage claim handling unit for Imperial. When it comes to GARD values he has implemented an agressive strategy to ensure his entire team is reflecting them each day. First all L! employees have undergone GARD value and customer service training, next he built the GARD values into each employees objectives which they are graded/audited. Once the Level 1 unit went live Shawn created excellent daily and monthly reports which demonstrate the visible value of his unit to the rest of the claim organization as well as the enterprise. Since the level 1 unit went live on September 16 he has managed his teams performance and achieved some amazing results. Shawn personally reviewed aged inventory and released more than $250,000 in reserves back to the company. His teams results in the first 30 days showed a savings of over $300,000 in indeminity/severity dollars. Shawn has also achieved a reduction in cycle time of approximately 20 days. Shawn routinely works extended work days and played a critical role in the overall claim transformation. ', CAST(0x0000A27C00AD8BA9 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (58, N'Jayme Westbrook', N'Texas', N'201311', N'IMPERIAL\sharon.cox', N'She has does everything that you ask her to do and then some. She is always willing to help. Always has a smile on her face, here on time, works hard, ', CAST(0x0000A27C00B141B0 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (59, N'Ali Hernandez', N'Texas', N'201311', N'IMPERIAL\jorge.barraza', N'This person has gone above and beyond for a concerned customer. He has shown great skills and dedication to his work and co-workers. He knows his material and emphasizes compassion to the people we serve and the people we don''t. ', CAST(0x0000A27C00B1641C AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (60, N'ALICIA VIDAL', N'Florida', N'201311', N'IMPERIAL\teresa.barro', N'SHE''S A PERSON THAT ALWAYS TRY TO HELP THE CO-WORKERS .. SHE''S IN TOP OF THE ACTIVITIES IN THE COMPANY , AND SHE''S GREAT PERSON TO BE THE EMPLOYEES OF THE MONTH.', CAST(0x0000A27C00C04AAC AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (61, N'Joseph Daugereaux', N'Louisiana', N'201311', N'IMPERIAL\boydg', N'He is always so helpful whenever asked and has a very positive attitude



He is always accessible and dependable and always resourceful in getting the task at hand done. 



I have seen Joseph walking through our department on more than one occasion asking if any of us were having any problems or needing anything. This is over and above the call of duty.


He does everything with a smile. Never a grumble about anything. 
', CAST(0x0000A27C00DAC9BE AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (62, N'Michelle Bourque', N'Louisiana', N'201311', N'IMPERIAL\Billy.Durel', N'If Webster’s published a business dictionary it would have a picture of Michelle next to Teamwork and Customer Service.
Michelle is always willing to assist her customers, be it internal or external ones.  Not only will she do her best to avoid transferring someone to voice mail, she will first try to answer any questions she is capable of handling.  This is holds true for agents and policyholders alike.
Additionally, Michelle always makes herself available to help her coworkers.  She has never turned down a request to help when she was not focused on a task and will look to help others when there is a lull in her work.
', CAST(0x0000A27E009951AF AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (63, N'Sharon Cox ', N'Texas', N'201311', N'IMPERIAL\mike.hernandez', N'she''s one of the biggest team players here, didn''t see anyone more excited about how our company is moving forward then her. she very efficient in her work takes it very serious. I see her checking in on her employees at least once a day just to see how everything''s going. and I know if you asked anything from her you can depend on her to do whatever it was and go above and beyond. she deserves the recognition and to be the employee of the month. ', CAST(0x0000A27E00E04DB1 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (64, N'Maria Brito', N'Florida', N'201311', N'IMPERIAL\luisa.perez', N'effective worker 




















































































































', CAST(0x0000A28100AD56F0 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (65, N'Laurie Nicholson ', N'Louisiana', N'201311', N'IMPERIAL\vickeyt', N'Laurie has always gone out of her way to help the insured''s/claimants when speaking with them on the phone. I have heard her look for claims number in order to transfer the call when the caller had very little information to go with. She always gives them the information needed prior to transferring the call. I have heard her console people that called in to report a bad accident. She seems to have a way to settle the callers down prior to transferring them.  

She is always happy to help each of us in the Bossier City office with running reports, settting up files, copying files when needed. 

She helps the Dallas office all the time also.

She is a very Plesent person to work with. ', CAST(0x0000A28100AF2D5F AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (66, N'Iliana Sanchez', N'Florida', N'201311', N'IMPERIAL\yan.sanchez', N'I''m nominating Ileana because I believe she gives by far the best customer service, she is always willing to help out anyone on the phone. 

Ileana exemplifies the GARD Values because she always treats everyone the same and cares about each claim, she is beyond well knowledgeable in what she does and is very dependable always on time and just an all around good person to be around and work with, 

I Believe Ileana goes above and beyond when dealing with each claim she takes time with each person to best help them out.  


She provides great CS because she is polite never loses her cool and treats everyone the same way. She''s always ready to answer and call that comes in for her.
', CAST(0x0000A28100B317F3 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (67, N'LEYDI MORALES', N'Florida', N'201311', N'IMPERIAL\marta.villasuso', N'this yound lady is always ready to help either customer or pier, she always does it with a smile in her face and she knows just about everything anyone asks.
she is young but her responsability is way beyond her years
I truly beleive she represents all Imperial wants of all their employees:
great service
courtesy
knowlege &
frienship', CAST(0x0000A28100B34688 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (68, N'Sue Willis ', N'Louisiana', N'201311', N'IMPERIAL\phyllisc', N'Sue is a very seasoned employee.  She has the insight to narrow litigation cases down to the root problem and work through the problem to reach a resolution/conclusion.  She is very focused, intelligent, competitive and driven. Through these qualities, our GARD values are exemplified in every way. Sue has never hesitated to take a leadership role in any task assigned to her.  She worked closely with IT in the last few years to apply data to the claims reports and test for accuracy.   There were many inconsistencies and Sue was strategic in making certain the claims reports were balanced with our accounting reports to the penney.   She always has the Company''s best interest in mind.', CAST(0x0000A28100B7EF22 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (69, N'Maria Eusse', N'Florida', N'201311', N'IMPERIAL\alex.gonzalvez', N'1-Maria has been a loyal employee for years. She is the perfect example of what hard work and determination can help one achieve. 
2-She started as a receptionist, then was promoted to Customer Service/1st Reports, and finally, after all of her hard work, Maria became a Property Damage Adjuster. 
3-Maria is someone that everyone can count on, considering that she is very knowledgeable of all processes a claim has to go through, not just the adjustment of the same.
-If for any reason an adjuster cannot assist one of our customers on a timely manne, Maria is always willing to lend helping hand, making her an extremely realiable person. 
4-Lastly, she often works overtime to keep all of her file on point and to provide all of our customers an exceptional service.
', CAST(0x0000A28100B9F251 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (70, N'Lisa Lowe', N'Florida', N'201311', N'IMPERIAL\andres.lopez', N'She''s a leader, She would help everyone else regardless of what she''s doing, she would go the extra mile to see that work gets done correctly and she will motivate you to do your best always.', CAST(0x0000A28100D93811 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (71, N'Lisa Lowe', N'Florida', N'201311', N'IMPERIAL\helen.obregon', N'Great Leader and is always very helpful and caring. 


















































', CAST(0x0000A28100DBA1DD AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (72, N'Sharon Cox ', N'Texas', N'201311', N'IMPERIAL\Pamela.Banks', N'Sharon, 
Has shown that she is up to the challenge, for making sure that her team is the best that they can be. Always encouraging us to ask question and to make sure we have a great environment to come into', CAST(0x0000A28100FED757 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (73, N'ZOLIA PRUDHOMME', N'Louisiana', N'201311', N'IMPERIAL\tamarica.trent', N'She goes above and beyond to make sure her job is completed to the best of her ability. She''s always willing to help others and learn new things. She''s pleasant to all she encounters daily.', CAST(0x0000A28101135FD7 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (74, N'Chris Jones', N'Texas', N'201312', N'IMPERIAL\Adam.Scott', N'Chris Jones exemplifies Imperial’s GARD values each and every day.   He consistently exercises good faith and fair dealing with internal and external clients. 

Chris is accessible and has no issues assisting others.  I called one day regarding an estimate which appeared overwritten by an independent appraiser (clm#73477).  Chris volunteered to visit the shop as he would be nearby that day and confirmed which repairs were made.  After discussions with the shop, Chris determined that a $200 battery was not replaced on this vehicle thus we had the independent remove it from their estimate.  Even though this was not an estimate Chris prepared, he was ready to step in and assist. This was definitely above and beyond the call of duty.

Resourcefulness is one of Chris’ strong points.  He has volunteered to assist management with the training of new field staff when his territory was slow. With Adam in Oklahoma, Larry in NOLA, Chris’ efforts in Baton Rouge assisting with training are definitely be considered above and beyond the call of duty.

In addition, we can depend on Chris to make the right decision while beside the vehicle.  He has been involved in multiple claims warranting SIU investigation and has recently had one referral (clm# 75409) where he has identified a possible fraud ring.
', CAST(0x0000A29300DCA3AA AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (75, N'Jill Schmalholz', N'Texas', N'201312', N'IMPERIAL\jbaker', N'Jill is very dependable and brings a positive attitude to the office every day. She is willing to do whatever is needed and has shown a great ability to learn new things. She has proven herself as a great asset to the underwriting team on more than one occasion.', CAST(0x0000A29800F10FF7 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (76, N'Jonas Valdez', N'Texas', N'201312', N'IMPERIAL\trey.robinson', N'Experienced leadership and no end to supporting the team with his knowledge and experience. Very supportive of upcoming ideas as well as bringing to the table even more on top of great ideas presented by the team. An Asset to the infrastructure team.', CAST(0x0000A29A0095AE5F AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (77, N'Cindy Nezat', N'Louisiana', N'201312', N'IMPERIAL\michelle.bourque', N'She is what makes this office---she is always running around and she gets things done. She is always there to listen and she really really really cares for Imperial ---the company and the staff members. She always makes sure that we get everything we need. She provides great Customer Service just by being there and always with a smile on her face. She deserves this so much', CAST(0x0000A29A00961ED5 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (78, N'Elizabeth Alayon', N'Florida', N'201312', N'IMPERIAL\rosana.mecias', N'She is a very hardworking person, she is a team player and she''s always willing to learn new things and help others. She has been an excellent Pip assistant for more than three years in the Company.', CAST(0x0000A29A009719C7 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (79, N'Cindy Nezat', N'Louisiana', N'201312', N'IMPERIAL\Tammy.Ducote', N'Cindy goes over and beyond to help all of us at the Louisiana office.  She sets up anything that we have to get together for in the conference room, i.e. meetings or monthly conferences.  She is the person that makes sure our supplies are stocked in supply room.  She is never cross and always very nice, even though stressed.  She puts together our insurance packets and handles it all.  I definitely recommend Cindy Nezat as employee of the month.
She is a great person and great employee.', CAST(0x0000A29A00973CA9 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (80, N'Maria Brito', N'Florida', N'201312', N'IMPERIAL\martha.sanchez', N'Maria is an excelent employee, always on time, handle her duties in a very professional manner and reach excellent
settlement in the handle of auto claims, she goes over and beyond her duties.', CAST(0x0000A29A00979AE7 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (81, N'Christopher Jones', N'Texas', N'201312', N'IMPERIAL\larry.ferrell', N'Since Chris Jones had been hired he has been a great resource for his fellow employees. Even though Chris was hired as an appraiser, he has already volunteered on numerous occasions to assist with training the Field Staff in Louisiana. Additionally, Chris has traveled to Dallas and New Orleans  to assist with field assignment overflow. Chris is a valuable resource and is looked upon as a mentor for guidance by fellow field appraisers. He represents Imperial''s GARD values on a daily basis through good faith and dealing not only with our customers, but his peers as well. Chris has a strong passion and desire to perform and represent Imperial in a positive light each and every day. He is extremely dependable and has already shown that he can be counted on in tough times on short notice when business needs arise.  ', CAST(0x0000A29A0097ABA4 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (82, N'Faye Gautro', N'Louisiana', N'201312', N'IMPERIAL\darla.pitre', N'She is a person that makes sure that her work is complete and also that it is done correctly.
She will always ask if there is something she can help with if she has any extra time on her hands.
She is always positive and friendly on phone calls and to co-workers.', CAST(0x0000A29A0097E4FD AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (83, N'Sandra Delgado', N'Florida', N'201312', N'IMPERIAL\jose.hernandez', N'Sandra Delgado is a perfect example of the perfect employee. She''s a PIP supervisor and going "over and beyond the call of duty" is just an understatement. She''s always available to help and give advise to her team and other co-workers. She is extremely professional but not everything is business for her - she knows how to balance professionalism and friendship. She''s a great mentor, great person, and great employee. I am forever in debt to her. She deserves this nomination more than anyone I know.', CAST(0x0000A29A009875A9 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (84, N'LISA LOWE', N'Florida', N'201312', N'IMPERIAL\andres.lopez', N'OUT GOING, WILL DROP WHAT SHE''S DOING TO HELP OUT. SHE WILL GO OVER AND BEYOND THE CALL OF DUTY, SHE WILL MAKE SURE TO HELP IN ANYWAY POSSIBLE. SHE''S A LEADER AND GOOD EXAMPLE TO FOLLOW. I''M GLAD TO HAVE HER ON MY TEAM.', CAST(0x0000A29A0098C669 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (85, N'Cindy Nezat', N'Louisiana', N'201312', N'IMPERIAL\christine.hawkins', N'I would like to nominate Ms. Cindy; 
There is nobody in this Office that is as dedicated and hard working as Ms. Cindy. There is really no words to describe everything that she does for the workers and even customers (at times). Ms. Cindy is always on the lookout for ways to create a better working environment and encourages us to do our best each and every day. Ms. Cindy always has an ear for us and will work with you until there is a solution for your problem. She get things done! She does not play around. I believe that Ms. Cindy is a great example for IFAC.', CAST(0x0000A29A009A6BF9 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (86, N'Martha Sanchez', N'Florida', N'201312', N'IMPERIAL\teresa.barro', N'I nominated Martha Sanchez, because she''s a great person , she''s very niece with all her co-workers ..  and help her departments and created a niece environment. 
I think she''s the best person to nominated for the employee of the month.. 
       ', CAST(0x0000A29A009CF230 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (87, N'Cindy Nezat', N'Louisiana', N'201312', N'IMPERIAL\stacey.morgan', N'Cindy always has a great attitude.  She is always responsive when anyone has a question.  If she doesn''t have an answer to a question, she does all she can to get an answer. I can''t help but smile when I talk to her, which makes Imperial a fun place to work.', CAST(0x0000A29A009F79B0 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (88, N'Allen Rasbury', N'Louisiana', N'201312', N'IMPERIAL\phyllisc', N'Allen is our complex claims adjuster.  In addition to his full litigation workload, he takes complex cases that require quite a bit of additional time and a very high level of experience.  He has quite a few complex cases reassigned to him each week and he takes it all in stride, handling each case with great expertise and professionalism. He is a very strong key employee in our claims department.   ', CAST(0x0000A29A00A001FE AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (89, N'Raul Amador', N'Florida', N'201312', N'IMPERIAL\leydi.morales', N'He is very helpful, always taking care of all our IT needs even if he''s busy working on other projects for his department. A joy to work with, never a dull moment with him in the office.', CAST(0x0000A29A00A025C5 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (90, N'Cindy Nezat', N'Louisiana', N'201312', N'IMPERIAL\lynnb', N'Cindy''s GARD VALUES are so on point. 
G- Fairness for all; 
A- Accessible day and night. 
R - If she doesn''t have the answer she will find it to help 
    others. 
D - Always willing to help out and works effortlessly to 
     please all.  
Cindy has always gone over and above the call of duty, whether it is a holiday party, B-12 shots, employee benefits or any work task she is given.  She gives heart and soul and then more - whether it is a week-end or night. 

If a customer calls in to her with a complaint she will get to the bottom of whatever it is.  She can be loaded down with work and she will stop and help the customer. 

We have been so fortunate to have her smiling face in Opelousas and I am so thankful to her for all she has done not only for me but for Imperial as well. ', CAST(0x0000A29A00A14B5D AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (91, N'Gisela Sanchez', N'Florida', N'201312', N'IMPERIAL\abraham.sanchez', N'This person goes above and beyond the call of duty each and every day by taking on additional daily tasks. This individual provides great customer service by preparing all the files necessary for each adjuster to review and be able to help each claimant accordingly. ', CAST(0x0000A29A00BB8CC7 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (92, N'Lisa Lowe', N'Florida', N'201312', N'IMPERIAL\helen.obregon', N'She''s an excellent leader, very helpful. Will stop whatever she is doing to help you. Very knowlegable when it comes to all departments. Makes work a fun place to be in. ', CAST(0x0000A29A00CEB262 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (93, N'Ali Hernandez ', N'Texas', N'201312', N'IMPERIAL\Jimmy.Beaird', N'Ali is always ready to jump in and do what it takes to get the job done.  When we have been short a person on our team he is there to give a 110%.  He handles all calls  very professionally and gets all his calls done quickily and in a timely manner. He works very well with all the members on his team.    ', CAST(0x0000A29A01112D01 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (94, N'Jose Hernandez', N'Florida', N'201312', N'IMPERIAL\sandra.delgado', N'Jose has very good negotiation skills in claims handling and handles them fairly and the settlements are for very reasonable amounts.

He maintains his diary up to date currently handling the highest inventory of the entire department, handles his new assignments promptly and the claims transferred from other adjusters.

He provides excellent customer service to the extent that other adjusters look for his help in contacting certain attorneys because of his good business relationship with them.

I have just great things to say about Jose.', CAST(0x0000A29D0067C364 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (95, N'JOSE HERNANDEZ', N'Florida', N'201312', N'IMPERIAL\maria.perez', N'HE IS VERY DEDICATED TO HIS JOB. HE RESPONDS PROMPTLY TO ALL HIS CALLS. WHEN ASKED A QUESTION HE IS WILLING TO GIVE YOU THE INFORMATION. HE HAS A POSITIVE AND PROFESSIONAL ATTITUDE AT WORK.   ', CAST(0x0000A29D0085E518 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (96, N'Michelle Bourque', N'Louisiana', N'201312', N'IMPERIAL\Billy.Durel', N'
If Webster’s published a business dictionary it would have a picture of Michelle next to Teamwork and Customer Service.
Michelle is always willing to assist her customers, be it internal or external ones.  Not only will she do her best to avoid transferring someone to voicemail, she will first try to answer any questions she is capable of handling.  This is holds true for agents and policyholders alike.
Additionally, Michelle always makes herself available to help her coworkers.  She has never turned down a request to help when she was not focused on a task and will look to help others when there is a lull in her work.
Although there have been fairly recent changes in her role for support, she was always willing to help out.  While out of the office for several days, unexpectedly, she once again stepped up to the plate.  When a couple of issues arose she was there to assist the Property Claims Manger who was helping with my desk.  Upon my return he commented that Michelle is a huge asset.
', CAST(0x0000A29D0094B577 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (97, N'Maria Eusse', N'Florida', N'201312', N'IMPERIAL\yan.sanchez', N'Shes an amazing adjuster and knows her job and duties well. Performs at 100% at all times and is willing to go the extra mile if need be. She is an excellent Example for future adjusters and I always look forward to working with her. ', CAST(0x0000A2A000977666 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (98, N'Tim Maher', N'Texas', N'201312', N'IMPERIAL\Laguilar', N'TIM HAS SHOWN US HOW HE HANDLES A CHALLENGE WHEN HE''S FACED WITH ONE. WE CHALLENGED LEVEL 2 ADJUSTERS TO EARLY SETTLE BODILY INJURY CLAIMS AND STARTED A CONTEST WHICH HE''S LEADING AND IS POISED TO WIN IT ONCE IT''S DONE. 
TIM IS ALWAYS APPROACHED BY OTHER ADJUSTERS OF DIFFERENT LEVELS AND ASK HIS OPINION ABOUT CLAIMS AND HE''S ALWAYS OPEN TO PROVIDE GUIDANCE TO ADJUSTERS. 
TIM HAS EXTENSIVE EXPERIENCE IN THE INSURANCE INDUSTRY AND HIS LEVEL OF CUSTOMER SERVICE SHOWS HIS ABILITY TO HANDLE EVEN THE MOST COMPLICATED PHONE CALLS. CUSTOMER COMPLAINTS ARE NOT AN ISSUE WITH TIM AND HE GOES BEOYND WHAT IS REQUIRED TO PROVIDE EXCELLENT CUSTOMER SERVICE BOTH INSIDE AS WELL AS OUTSIDE IMPERIAL.
TIM IS RISING STAR AT LEVEL 2', CAST(0x0000A2A00098C01A AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (99, N'Christine Hawkins', N'Louisiana', N'201312', N'IMPERIAL\michelles', N'Christine is the type of employee that goes the extra mile in whatever task is before her, be it handling the total loss claims, assisting the adjusters by translating or speaking with customers and getting them the answers they need.  She is quick about her work and moves on to the next task at hand.  There have been numerous times she has been able to address the concerns of my customers without having to put the call through to me and is always willing to assist no matter what I ask of her.  I have NEVER heard anyone complain about an experience with Christine.  She is genuine and sincere and a true asset.  ', CAST(0x0000A2A0009B7002 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (100, N'Jose Hernandez', N'Florida', N'201312', N'IMPERIAL\Shady.medina', N'Jose is a excellent team worker, responsive and honest.

He works hard all day and he is excellent in what he does.

He listens to his customer, helps them and also always in track of everyone''s needs.
', CAST(0x0000A2A000A0CDAF AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (101, N'Guillermo Callejas', N'Florida', N'201312', N'IMPERIAL\Carmen.Lambert', N'Guillermo has been asked to take over an essential role in the Miami Claims office and has done so with positive attitude and great work ethic. The EUO Coordinator position is complicated and requires a highly organized, knowledgeable and skilled employee. Guillermo meets all of these requirements and goes above and beyond the call of duty.  ', CAST(0x0000A2A000ED5E36 AS DateTime))
GO
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (102, N'Jose Hernandez', N'Florida', N'201312', N'IMPERIAL\juan.delgado', N'Jose, meets and exceeds all his work related duties. He also goes beyond and above his assigned duties.

Jose provides all the proper documentation on time and accurate.

Jose obtains the proper information from insureds for EUOs.', CAST(0x0000A2A1006CF45B AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (103, N'Martha Sanchez', N'Florida', N'201312', N'IMPERIAL\alicia.vidal', N'I would like to nominate Martha Sanchez for our December''s EOM.  Martha Sanchez is an amazing BI Adjuster. Martha is one of our young senior citizens here and in her 70s but sharp as a whistle.
Martha is extremely knowledgeable and experienced of close to 30 years as an Adjuster.
Martha always says yes when asked for assistance. She goes out of her way to help you and is readily available to answer questions or situations requiring her expertise.  
Although not assigned claims, she multi-tasks as an adjuster to assist other adjusters.  She goes above and beyond her call of duty. Martha assists when other adjusters are on PTO for one or multiple days.
Martha is seen as a mentor to other adjusters.  Often, she is consulted with as her business knowledge and contacts are very helpful in negotiating claims as many attorneys have long standing relationships with Martha.
Her customer service skills both within and outside the office are excellent.  You will never find Martha in a bad mood.  Martha possesses a great personality and a wonderful sense of humor.
Martha is a true example of the GARD Values.', CAST(0x0000A2A100F4949B AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (104, N'CHASSIDY FORD', N'Louisiana', N'201312', N'IMPERIAL\zoilap', N'good customer  service very nice with co workers always smiling 
very pleasant to work with her 
always doing things beyond their job 
alway help with any question or any matter when you need help', CAST(0x0000A2A10110E555 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (107, N'Michelle Bourque', N'Louisiana', N'201401', N'IMPERIAL\Billy.Durel', N'If Webster’s published a business dictionary it would have a picture of Michelle next to Teamwork and Customer Service.
Michelle is always willing to assist her customers, be it internal or external ones.  Not only will she do her best to avoid transferring someone to voicemail, she will first try to answer any questions she is capable of handling.  This is holds true for agents and policyholders alike.
Additionally, Michelle always makes herself available to help her coworkers.  She has never turned down a request to help when she was not focused on a task and will look to help others when there is a lull in her work.
Although there have been fairly recent changes in her role for support, she was always willing to help out.  While out of the office for several days, unexpectedly, she once again stepped up to the plate.  When a couple of issues arose she was there to assist the Property Claims Manger who was helping with my desk.  Upon my return he commented that Michelle is a huge asset.
', CAST(0x0000A2C000A84908 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (108, N'JILL SCHMALHOLZ', N'Texas', N'201401', N'IMPERIAL\lrodriguez', N'Jill keeps going over and beyond for Imperial.  Works really hard to keep our incoming undewritting information current.  Provides excellent customer service and really cares about our customers and agents.  She has steped up to the plate and is now helping Julie with accounting.  ', CAST(0x0000A2C000A95D80 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (109, N'TAMERICA TRENT', N'Louisiana', N'201401', N'IMPERIAL\fayeg', N'SHE GOES OVER AND BEYOND THE CALL OF DUTY, BY HELPING THEM
AND SHE WILL MAKE CALLS FOR THEM AND CALL THE PERSON BACK,
SHE WILL WORK STEP BY STEP WITH THEM ALL THE WAY. SHE WILL
RESET PASSWORDS FOR THEM AND CALL THEM BACK WITH THEIR NEW PASSWORDS.', CAST(0x0000A2C000A96662 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (110, N'Iliana Sanchez', N'Florida', N'201401', N'IMPERIAL\yan.sanchez', N'Iliana Is a very dedicated employee always knows to do her job well and never has any pending work. she is diligent with her job and knows how to do it exceptionally. ', CAST(0x0000A2C000A9E97C AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (111, N'Andres Lopez', N'Florida', N'201401', N'IMPERIAL\lisa.lowe', N'Andres Lopez always is willing to help anyone.  He is a very talented employee and goes above and beyond.  When anyone has a problem with the printer or scanner located next to his desk he will stop doing what he is doing and fix the machine.  Andres always answers his phone and handles the calls coming into our office with professionalism and respect.  He is willing to help the person on the phone and not just dump the call. ', CAST(0x0000A2C000B1B823 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (112, N'Adam Scott', N'Texas', N'201401', N'IMPERIAL\Brad.Morrisett', N'Adam is one of the best supervisors I have ever worked for. He is always accessible. He answers my questions in a timely manner. He is also very resourceful. He has the ability to help me write estimates and come up with ways to make the owner of the car happy and the company. He always has the company and insured''s best interest at heart. I enjoy working for him.  ', CAST(0x0000A2C000B3EAB1 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (113, N'Amalia Castro ', N'Florida', N'201401', N'IMPERIAL\maria.brito', N'Even though Amalia has already been nominated for employee of the month I feel she continues to do an exelent job. her desk is always up to date and she always treats the clients with respect. ', CAST(0x0000A2C1006A5560 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (114, N'Mike Hernandez ', N'Texas', N'201401', N'IMPERIAL\Jimmy.Beaird', N'Mike has been working with us for about 7 months and he is responsible for all of our doc ops.  He always makes sure that all of our faxes, emails, incoming/ out mail is done. He is the person we call on to move anything in our department.  He always does this with the best attitude and with a smile.  He is wlling to do whatever it takes to get the job done as quickly as possible.  He is a value to our department and he is here everyday on time.  I am very grateful to have someone so dedicated to this position.  ', CAST(0x0000A2C2009007E9 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (115, N'Robert Freire', N'Florida', N'201401', N'IMPERIAL\rosana.mecias', N'Roberto is an excellent supervisor. He is a hardworking person and a team player. He is always willing to help his co-workers and he always do a great job as a Controller and as a Human Resource manager.', CAST(0x0000A2C20106BA76 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (116, N'Jill Schmalholz', N'Texas', N'201401', N'IMPERIAL\pzimmerman', N'Because she goes above and beyond her job duties daily.

She is honest, loyal, and dependable and always shows respect to others. She always has a smile and cheerful greeting to all. Many agents and insured''s have complimented on the way she has handled an issue.

She resolves issues in a timely manner. On and off the phone she is kind and thoughtful.
', CAST(0x0000A2C30093562A AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (117, N'Jill Schmalholz', N'Texas', N'201401', N'IMPERIAL\posborn', N'She goes above and beyond her regular duties to solve an issue.

She is curteous on the phone.  When a challenging situation
arises on the phone, she meets the adversity with a smile and handles the issue.  

She is dependable and cheerful and a great co-worker.  When needed  she ''steps up to the plate''.', CAST(0x0000A2C30096019B AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (118, N'Maria Brito', N'Florida', N'201401', N'IMPERIAL\luisa.perez', N'She is a reliable worker, proactive and direct. 
Maria Brito is polite and clear when she request any task. 
She is knowledgeable and she is willing to teach at any time.  ', CAST(0x0000A2C400989E0D AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (119, N'Maria Brito', N'Florida', N'201401', N'IMPERIAL\isabel.andres', N'She is a very responsible person, accurate at what she does (BI adjuster), punctual, always willing to cooperate & help her fellow workers. Even when she is occupied & somebody asks for her help, she will stop what she is doing & will help in any way she can.
She deals with attorneys all the time and conduct herself in a very professional way, she provides excellent customer service in all her calls.', CAST(0x0000A2C4009A001F AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (120, N'Teresa Barro ', N'Florida', N'201401', N'IMPERIAL\marlene.fernandez', N'Teresa is a pd adjuster ,I have worked with her for the past few years , she has always shown to be very thorough when working her claims files, along with underwriting verifying coverages and any other information to make sure all claims are paid properly , looking into any possible fraudelent issues and looking out for the best interest of the company as well as satisfying our insured with her service.      ', CAST(0x0000A2C4009C2075 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (121, N'Stacey Morgan', N'Texas', N'201401', N'IMPERIAL\Cassandra.Cline-Gera', N'Stacey goes "over and beyond the call of duty" every day.  She is always there to answer a question, or give an opinion...even to lend a fresh pair of eyes to a problem.  She strives to make Imperial the best it can be.  ', CAST(0x0000A2C4009C2A4A AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (122, N'Cary Machado', N'Florida', N'201401', N'IMPERIAL\maritza.morera', N'A very hard worker who takes extra care to make sure that her clients & team work are satisfied. She consistently demonstrates a positive attitude, working toward the team goals. Perseverant and committed to doing her best in the accomplishment of the Co. goals. She exceeds the expectations of her work.', CAST(0x0000A2C4009E443C AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (123, N'Jill Schmalholz', N'Texas', N'201401', N'IMPERIAL\jbaker', N'We received compliments about Jill for wonderful customer service this month from several of our agents. I have also found her to be very dependable and helpful with anything she is asked to do. She is willing to help out any way she can to provide great customer service. She always has a great attitude and that reflects on her customer and employee interactions. Once again she has proven herself as a great asset to the underwriting team this month.', CAST(0x0000A2C400A2AD8A AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (124, N'Nancy Thibodeaux', N'Louisiana', N'201401', N'IMPERIAL\Tammy.Ducote', N'Nancy is a very hard-worker and dedicated to IFAC.  When I have come in on a weekend, she is most of the time here, also.  She works late in the evenings as well to get her job done.  She is always so helpful and very kind to co-workers.  If we in Claims do need help in any type of Accounting matter, she is always there to do anything she can to assist us.  I highly recommend Nancy Thibodeaux for employee of the month for January, 2014.', CAST(0x0000A2C400ABCBEE AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (125, N'Pat Osborn', N'Texas', N'201402', N'IMPERIAL\lrodriguez', N'Pat Osborn has been with Imperial for about 9 years.  She truely cares about our policy holders and agents. Goes over and beyond and steps up to the plate when needed. She is curteous on the phone.  When a challenging situation  arises on the phone, she meets the adversity with a smile and handles the issue. She is dependable and cheerful and a great co-worker.  Marketing Department advised every agent likes Pat very much and states she goes the extra mile to help them.  ', CAST(0x0000A2C900C63790 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (126, N'Cassandra Cline-Gerasimchik', N'Texas', N'201402', N'IMPERIAL\stacey.morgan', N'We have had multiple high priority projects this month, and Cassie is always offering to step in and assist.  She''s taken on three new tasks above her daily duties, and she works quickly and efficiently to get the job done.  She always has a smile and makes the office a fun place to be.', CAST(0x0000A2D800AFCCEE AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (127, N'Ali Hernandez', N'Texas', N'201402', N'IMPERIAL\Pamela.Banks', N'Ali really keeps the FNOL Team laughing, and upbeat. Besides all ways being willing to assist hi team mates with their questions no matter how there are, He does it with a smile. I believe that he is really a great representation of Employee of month. ', CAST(0x0000A2D90107A284 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (128, N'Faye Gourto', N'Louisiana', N'201402', N'IMPERIAL\melissad', N'Faye  has the best attitude no matter what she is asked to do she does it to be best of her abilities, she always has a smile on her face on a kind word for everyone', CAST(0x0000A2D901082778 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (129, N'Nancy Thibodeaux', N'Louisiana', N'201402', N'IMPERIAL\Olivia.Williams', N'Nancy deserves this simply because she is a great rold model when it comes to work. She is great with customers on the phone, she helps anyone who needs anything from her. She will go out of her way to give you an answer if she does not have one at the time. All around, she is a great person! I love working with such great co-workers. But of all, I believe that Nancy is the one who deserves this! ', CAST(0x0000A2D9010841B5 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (130, N'Will Beason', N'Texas', N'201402', N'IMPERIAL\michael.williams', N'I would like to recommend that Will Beason (Arkansas and Oklahoma State Manager) be considered for Employee of the Month.

Having a strong network of agents is imperative to succeeding in any territory and Will has done a masterful job of creating and maintaining strong relationship with our Arkansas agents.  And as a new employee working on my first Arkansas rate revision, Will has been a great resource.  He is not only knowledgeable about our agents, but he is also very knowledgeable about our customer base and competition.  Although Will is often marketing in the field, he is very accessibly and is regularly providing feedback and offering suggestions.  I was particularly impressed with the flexibility that he showed when he had to “re-market” our rate revision after challenges with the Department of Insurance required changes and delays.

I feel that Will has a genuine passion and energy about representing Imperial and helping our product grow profitably in Arkansas.  And I think he is deserving of consideration for Employee of the Month.', CAST(0x0000A2D9012CC24B AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (131, N'Lewis De La Fuente ', N'Florida', N'201402', N'IMPERIAL\maria.brito', N'Lewis is the appraisal department supervisor. He is very professional. He treats everyone with respect and does not walk away from solving any problems.', CAST(0x0000A2DC0062D5DA AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (132, N'Maribel Humaran', N'Florida', N'201402', N'IMPERIAL\lisa.lowe', N'Maribel has the Gard Values.  She is always helpful and always friendly.  She cares about the people she works with and exemplifies the ideal employee.  
Maribel''s strength is her positive attitude and her attitude is what she uses to get her job done.  She shows you that you can be sweet and still achieve the same end results as adjusters who are aggressive.', CAST(0x0000A2DC007BBFE4 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (133, N'Juan Penna', N'Florida', N'201402', N'IMPERIAL\Shady.medina', N'He is an excellent worker, I could honestly say Juan is one of the best people I know, he is honest, trustful, respectful, loyal, smart, capable of be someone we could look up to. He represents the company in the best way, always professional.  
Juan gives the best of him everyday not just to customers also to us his coworkers, he is always willing to help solving any problem or task, always with a smile, courteous and a great attitude, he never says no he always finds a solution to every obstacle he has.  If we say over and beyond the call of duty we are definably describing Juan.
', CAST(0x0000A2DC007F4996 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (134, N'Pat Osborn', N'Texas', N'201402', N'IMPERIAL\pzimmerman', N'Because she goes above and beyond her job duties daily.

She is loyal, dependable, and honest and always shows respect to others. She always has a smile and cheerful greeting to all. Lots of agents have complimented on the way she has handled an issue.

We recently had bad weather which prevented most from getting out before daylight or even coming to work, but she was here before 6:30 processing. ', CAST(0x0000A2DC00824A0F AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (135, N'Maria Brito', N'Florida', N'201402', N'IMPERIAL\marlene.fernandez', N'I have worked with Maria for a few years, she is a very caring and friendly person, always greeting with a smile and positive attitude.she is also a very hard worker , very detalied and focused when working her files , Maria is a "perfectionist" always looking out for the best interest of the company as well as making sure the insured is sstisfied and treated properly during the process of the claim.    ', CAST(0x0000A2DC008618B1 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (136, N'Christine Hawkins', N'Louisiana', N'201402', N'IMPERIAL\michelle.bourque', N'Christine does wonderful customer service with the Salvage and not only that; she helps the adjusters translate Spanish. She goes over & beyond to help the customers and the adjusters out. ', CAST(0x0000A2DC00959A00 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (137, N'Fernando Rodriguez', N'Florida', N'201402', N'IMPERIAL\maribel.humaran', N'Fernando is a young charismatic man. He strives to do well and exceeds in all the projects given to him.  He is a respectful and has great willingness to learn. Fernando is an example of what the GARD values are.
', CAST(0x0000A2DC00DD278E AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (138, N'Maria Brito', N'Florida', N'201402', N'IMPERIAL\isabel.andres', N'Maria is a BI adjuster. She deals with attorneys & customer in a very professional way. Her work handling claims is very accurate, she is always on time & when one of her co-workers need help on a claim, she is always willing to help.', CAST(0x0000A2DD00570A90 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (139, N'ELDRY MEJIA', N'Florida', N'201402', N'IMPERIAL\gladys.ruiz', N'I AM VERY HAPPY TO HAVE A CO-WORKER WITH ALWAYS A GREAT ATTITUDE.  HE IS ALWAYS WILLING TO GO OUT OF HIS WAY WHEN HELP IS NEEDED.  THIS IS WHAT MAKES IMPERIAL INSURANCE COMPANY A WINNER CO-WORKERS LIKE HIM ARE A MUST.', CAST(0x0000A2DD006DE173 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (140, N'Nancy Thibodeaux', N'Louisiana', N'201402', N'IMPERIAL\Tammy.Ducote', N'I am nominating same person as I did last month, Nancy Thibodeaux.  She is always willing to help with no complaints at all, appears to come in during off hours and has been an employee of the company for many years.  She is an awesome employee and person.', CAST(0x0000A2DE00ADEB50 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (141, N'Pat Osborn', N'Texas', N'201402', N'IMPERIAL\jill.schmalholz', N'Pat Osborn is an exceptionally dependable team player.
During the recent winter weather, she arrived early,ready to make sure the phones were manned.
She is always flexible with  co-workers for any schedule changes, usually the first to volunteer.
She extends through her calm demeanor , professionalism and fairness in interaction with both policyholders and agents.', CAST(0x0000A2DE00F14FEC AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (142, N'Nancy Thibodeaux', N'Louisiana', N'201402', N'IMPERIAL\scottp', N'Especially in a month where all year-end reporting is due in addition to the normal crunch of daily accounting, Nancy continues to be her normal cordial, friendly and helpful self.  She always makes time for you when needed.

Her dedication to Imperial is always evident in her commitment to getting her job done.', CAST(0x0000A2DF00BA828B AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (143, N'Zoila Prudomme ', N'Louisiana', N'201402', N'IMPERIAL\chassidy.Ford', N'Always helpful when needed for a Spanish call and is always pleasant to speak with. She has a smile on her face to assist you when help is needed.IAlways friendly and nice when you meet up with her.', CAST(0x0000A2DF00BA8AE0 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (144, N'Cindy Nezat', N'Louisiana', N'201402', N'IMPERIAL\dirkb', N'During the transition of the employee benefit plans from the current broker to the new broker in Texas, Cindy did an outstanding job in coordinating all of the enrollments.  In addition, she also provided a great deal of assistance in the successful payment of all employees on the first payroll of 2014, which included the Freestone and National Automotive employees.  When asked for assistance, her answer is always how can I help.  That makes her a great example of our core values.', CAST(0x0000A2DF00BB4732 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (145, N' Ali Hernandez ', N'Texas', N'201402', N'IMPERIAL\mike.hernandez', N'goes above and beyond to get the job done. helps out the team when they need help. always willing to stay late when we need it even if its on a short notice. knows how to keep the team in a cheerful mood. ', CAST(0x0000A2DF00BBADC6 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (146, N'Faye Gautrout', N'Louisiana', N'201402', N'IMPERIAL\darla.pitre', N'Faye is a hard worker and helps anyone who needs help she is always friendly and always has a smile on her face.  She stays busy and ask often if there is anything she can do to help others.  Its nice to work with someone who is so kind.', CAST(0x0000A2DF00BBCFC2 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (147, N'Ryan Selby-Karney', N'Texas', N'201402', N'IMPERIAL\Andrea.Ward', N'Ryan is the kind of employee you dream about having on your team.  He performs every task with purpose and with a smile.  Agents rave about the service that he provides.  Last month, Ryan didn''t hesitate to stay late to cover the phones several times when a co-worker was out unexpectedly.   He has only been part of the Imperial family for a few months, but his positive attitude and friendly demeanor have made a big impact on our department.  ', CAST(0x0000A2DF00BBF2A8 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (148, N' Linda Peel', N'Louisiana', N'201402', N'IMPERIAL\michelles', N'Linda is a hard worker and very dependable.  She has a wealth of knowledge and does not mind helping out when needed.  Linda has worn several "hats" since we started here and is an asset.    ', CAST(0x0000A2DF00C0896D AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (149, N'Mike Hernandez ', N'Texas', N'201402', N'IMPERIAL\Jimmy.Beaird', N'Mike is a major support to our entire claims team.  He makes sure all faxes, emails, incoming / outgoing mail is processed in a timely manner.  Mike is always very willing to get the job done when ask.  he trys his hardest when times are hardest.  he takes on all new responsibilities with a smile and is very interested in learning new task.  cant as for a better person to have at Imperial. 

thanks 
:-) ', CAST(0x0000A2DF00C0FCB5 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (150, N'Roberto Jorge', N'Florida', N'201402', N'IMPERIAL\Carmen.Lambert', N'Roberto is always willing to step up and assist whenever and wherever he is needed. He agrees to fill in for a sick adjuster and investigator and he takes on whatever task we ask of him. He takes Examinations Under Oath for our legal team and he goes above and beyond to ascertain information from his various contacts and resources to assist in the defense of our lawsuits and/or the investigation of our claims. I am lucky to have him on my team for many reasons. Of all the extra effort Roberto puts in to his daily job the fact that he is kind and thoughtful to our new employees, is the attribute I am most fond of.  ', CAST(0x0000A2DF00C1794B AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (151, N'Robert Freire', N'Florida', N'201402', N'IMPERIAL\rosana.mecias', N'Robert is an amazing co-worker and supervisor. He is always willing to help others with a smile on his face.He is a hardworking person and a team player. I believe he should be the employee of the month because he deserve it.  ', CAST(0x0000A2DF00D4F074 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (152, N'Maria Brito', N'Florida', N'201402', N'IMPERIAL\amalia.castro', N'Maria is always willing to help other. She is very punctual. Very Professional when it comes to handle claims with attorney''s and insured''s. She is always cut up with her daily work.', CAST(0x0000A2DF00DA3ED2 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (153, N'MARLENE FERNANDEZ', N'Florida', N'201402', N'IMPERIAL\maria.perez', N'MARLENE ALWAYS GIVES THE NECESSARY INFORMATION WHEN PROMPTED WITH A QUESTION.  SHE PROVIDES INFORMATION IN A CORRECT AND PLEASANT MANNER.  SHE ANSWERS ALL HER TELEPHONE CALLS, AND SEEMS TO HAVE VERY GOOD JOB PERFORMANCE.    ', CAST(0x0000A2DF00E1D944 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (154, N'Cassandra Cline- Gerasimchik', N'Texas', N'201402', N'IMPERIAL\Blanca.Carrillo', N'Cassie is a great colleague with such a wonderful attitude. Not only is she always pleasant to be around, but she is always accessible for help and a dependable person to work with.', CAST(0x0000A2DF0103CDD2 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (155, N'Linda Peel', N'Louisiana', N'201403', N'IMPERIAL\traciel', N'Linda Peel is very knowledgable in her expertise of handling claims. She is sometimes the first one hear and sometimes the last one to leave. She is dedicated to her work and helping others', CAST(0x0000A2E7010ED11B AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (156, N'Frances Diaz', N'Texas', N'201403', N'IMPERIAL\jstarnes', N'If there is anyone that exemplifies Imperials GARD Values it would be Frances Diaz.   

Frances goes out of her way to help the internal and external customers.  She has provided translation services with clients that speak spanish only. 

Frances is extremely dependenable, she always has a friendly voice, and an exceptional smile.  She is a joy to work with.  

It is very difficult to point out "one" over and beyond the call of duty occurance for her, as she consistently does this on a daily basis, with little to no recognition. 

I believe that Francis Diaz should be nominated as the employee of the month for the month of March 2014, as she is a consistent, long term employee, that needs to be recognized for not only her longevity with Imperial, but also her compassion, and her ability to be not only an outstanding employee but an outstanding human being, treating others as you would want to be treated. 

', CAST(0x0000A2F500A813EE AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (157, N'YAN SANCHEZ', N'Florida', N'201403', N'IMPERIAL\oscar.rueda', N'HE IS REALIABLE, HELPFULL, ALWAYS THERE FOR HELPING OTHERS
EXCELLENT CUSTOMER SERVICE. YAN WORKS IN THE APPRAISAL DEPT COORDINATING VEHICLE INSPECTIONS AND SUPPLEMENTS REQUEST.
EVERYTIME YOU CALLED YAN HE IS ALWAYS THERE FOR HELPING US ON ANY RELATED APPRAISAL ASSIGMENT AND ESTIMATE QUESTIONS. ', CAST(0x0000A2F800F33DD8 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (158, N'Lisa Irvine ', N'Florida', N'201403', N'IMPERIAL\maria.brito', N'Lisa has a great attitude. She is very helpful with the clients and is very approachable if you need her help. I have never seen her in a bad mood or raising her voice. She has a clear and respectful way of talking to the clients. ', CAST(0x0000A2F800F39DEB AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (159, N'Ves Garrett', N'Louisiana', N'201403', N'IMPERIAL\brendab', N'Ves is always willing to help.  He never gets upset with anyone.  He will help out in any way that he can and if he is not sure what needs to be done, he will research it to see how to get the issue resolved', CAST(0x0000A2F800F443DE AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (160, N'Ryan Selby-Karney', N'Texas', N'201403', N'IMPERIAL\Andrea.Ward', N'Ryan has an incredible can-do attitude and always goes above and beyond expectations.  This month, he has stayed late on several occasions to provide phone coverage when a team member needed to leave early.  Our marketing reps are always reporting back that our agents love working with Ryan.  While he has been with Imperial for just a short time, his infectious enthusiasm have made a big impact on our department.  His positive attitude and work ethic exemplify Imperial''s GARD values.', CAST(0x0000A2F800F71316 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (161, N'Mike Hernandez', N'Texas', N'201403', N'IMPERIAL\sharon.cox', N'Mike is a dedicated employee who has shown that he cares about his job and the people around him. He is there for the team willing and able. Always here on time, goes over and beyond the call of duty. Never has anything negative thing to say. I honestly wish I had more employees like him. ', CAST(0x0000A2F800F835E5 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (162, N'Frances Diaz', N'Texas', N'201403', N'IMPERIAL\Mike.Reyna', N'I had a Spanish speaking insured that I could not communicate with and she translated the call for me on the spot to provide excellent customer service and embodying the GARD values.', CAST(0x0000A2F800F8601E AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (163, N'Frances Diaz', N'Texas', N'201403', N'IMPERIAL\dheffelfinger', N'Frances is a pleasure to work with.  Always eager and willing to go the extra mile whenever help is needed.  You can always count on Frances to be friendly and cheerful. The professional manner in which she conducts herself to both our customers and fellow employees and her didication to service makes our company stand out from others.  Frances Diaz makes Imperial a better place to work and a better place to do business.', CAST(0x0000A2F800FCBCBC AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (164, N'Francis Diaz', N'Texas', N'201403', N'IMPERIAL\charles.crane', N'Francis is a dedicated and friendly employee. Her foremost concern is helping the Insured place their claim and ensures that the claim is routed to the right adjuster. She goes above and beyond by working additional time necessary to complete and gather all the right information from the Insured or claimant. Her empathy and tone convey her caring attitude towards our customers and claimants.
She is a role model for new employees to strive to her level of professionalism. 
She is the " best" claim intake person that I have ever seen or heard.', CAST(0x0000A2F80107450D AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (165, N'Maria Brito', N'Florida', N'201403', N'IMPERIAL\isabel.andres', N'Handling BI claims,she deals with attorneys & customers & conducts herself in a very professional way, she is very accurate & punctual. She is willing to help her co-workers when she is asked to, and in any way she can.

In my opinion, for the reasons above mentioned, she deserves to be the employee of the month.', CAST(0x0000A2F90056CBD6 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (166, N'Lisa Irvine ', N'Florida', N'201403', N'IMPERIAL\luisa.perez', N'she is reliable, efficient and knowledgeable.                                                       
she is always willing to work on her files as soon as I give them to her. ', CAST(0x0000A2F90072A2FC AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (167, N'Valentin Arenas', N'Florida', N'201403', N'IMPERIAL\Nikolas.Arenas', N'Valentin is one of those people in the office that you can count on for anything. Whether his desk is cluttered with files or clean as can be, Valen is always willing to lend a helping hand. He is a ray of sunshine with in the office that everybody loves. He is a perfect example of an EoM because, not only is he a coworker in the office but a friend as well. ', CAST(0x0000A2F9007EE4F0 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (168, N'Martha Sanchez', N'Florida', N'201403', N'IMPERIAL\teresa.barro', N'Martha is a very niece person, very professional, she''s very positive, very responsible , very friendly. For me and all my co-workers , she''s the best person to be the employee of the month     ', CAST(0x0000A2F9009625FD AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (169, N'Martha C. Sanchez', N'Florida', N'201403', N'IMPERIAL\fernando.rodriguez', N'This person exemplifys the GUARD values in numerous ways. Martha always makes sure that everything she does is done to the best of her abilities. Martha brings a positive attitude in the working enviorment and is always accessible to offer anyone in need a helping hand. Martha''s leadership plus her great character is model for anyone in the office to follow. In my opinion she deserves to be employee of the month becuase she is a symbol of our GUARD values. ', CAST(0x0000A2F9009BB60E AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (170, N'Martha Sanchez', N'Florida', N'201403', N'IMPERIAL\alicia.vidal', N'I would like to nominate Martha Sanchez, BI Adjuster for our March Employee of The Month. Martha Sanchez is an amazing BI Adjuster. Martha is one of our young senior citizens here and sharp as a whistle.  Martha is extremely knowledgeable and experienced Adjuster with 30 years of great service.  Martha always says yes when asked for assistance. She goes out of her way to help others and is readily available to answer questions or situations requiring her expertise.  Although not assigned claims, she multi-tasks as an adjuster to assist other BI adjusters. Martha assists when other adjusters are on PTO for one or multiple days.  She goes above and beyond her call of duty.  Martha is seen as a mentor to other adjusters.  Often, she is consulted with as her business knowledge and contacts are very helpful in negotiating claims as many attorneys have long standing relationships with Martha.  Her customer service skills both within and outside the office are excellent.  You will never find Martha in a bad mood.  Martha possesses a great personality and a wonderful sense of humor.  Martha is a true example of the GARD Values and should be recognized for her work.  
', CAST(0x0000A2F9009F1846 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (171, N'Dallas Underwriting And C/S', N'Texas', N'201403', N'IMPERIAL\wbeason', N'They class, effort, humility and composure that this group showed in some very difficult times was beyond measure. I cannot thank them enough and truly appreciate their efforts.

Will Beason', CAST(0x0000A2F900B0878A AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (172, N'Maria Perez', N'Florida', N'201403', N'IMPERIAL\marlene.fernandez', N'I have worked with Maria for a few years ,although not in the same department I have dealt with her when verifying coverages and other information on policies for claims . Maria is a very dedicated employee , always treating her peers with repect . she also covers the reception when needed and there also treating our insureds with courtecy as well as giving great customer service. ', CAST(0x0000A2F900D4616E AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (173, N'Dirk Boudreaux', N'Louisiana', N'201403', N'IMPERIAL\boydg', N'Dirk always has the interest all parties interest in mind when dealing with people, whether it is the customer, employee or anyone else. He shows respect to all people and puts others before self. He has an open door policy and has an ear to hear at all times. ', CAST(0x0000A2F900E5B5BE AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (174, N'Mike Hernandez ', N'Texas', N'201403', N'IMPERIAL\Jimmy.Beaird', N'Mike is a valuable employee to the claims team.  he is always ready to get any job done and here everyday on time.  he is always trying his hardest during times when the claims team needs him.  every new task that is given to him is completed in a timely manner.  he always shows a very positive attitude toward everyone and is friendly and very polite.  I feel that Mike Hernandez always practices Gard values each and everyday he is here.  Therefore, this makes him a valuable employee to the IFAC team.  ', CAST(0x0000A2FA007FDD52 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (175, N'Faye Goutro', N'Louisiana', N'201403', N'IMPERIAL\melanieb', N'Faye always has a smile and willing to help anyone. If anyone asked Faye for help she will never refuse if she has never done that job before she is willing to learn.', CAST(0x0000A2FA0083F95F AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (176, N'Mike Reyna', N'Texas', N'201403', N'IMPERIAL\pzimmerman', N'He is polite and willing to help. He has excellent customer service skills. He resolves things at his desk and is consistent month after month.
He had a fire claim called in today, and not only did he handle the claim information, but also had the restoration company at the scene to start the clean up. I would say that''s outstanding customer service.', CAST(0x0000A2FA00B498E4 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (177, N'Mike Reyna', N'Texas', N'201403', N'IMPERIAL\posborn', N'While he keeps the Company''s interest in focus he provides awesome claims service.  Whenever he deals with a claimant or insured, he is calm and courteous. He is very professional and friendly to co-workers.  ', CAST(0x0000A2FA00B89268 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (178, N'Maria Brito', N'Florida', N'201403', N'IMPERIAL\martha.sanchez', N'Handle her job as a BI adjuster in a professional manner,
punctuality throught the years and cooperation with any
field related with the handle of insurance claims.
We are proud of Maria Brito
', CAST(0x0000A2FA00DD1A89 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (179, N'Martha Sanchez ', N'Florida', N'201403', N'IMPERIAL\maribel.humaran', N'Martha is an inspiration to all of us at Imperial. She is always willing and ready for any task. She demonstrate initiative, high quality of work and a over all a great team player. ', CAST(0x0000A2FA00E83646 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (180, N'MIKE REYNA', N'Texas', N'201403', N'IMPERIAL\lrodriguez', N'Mike Reyna keeps going over and beyond for Imperial.  Works really hard to keep our incoming claims processed.  Provides excellent customer service and really cares about our customers. Mike is friendly and always has a smile. I have translated several Spanish calls for Mike this month and he went that extra mile to make sure the customers were taken of.  ', CAST(0x0000A2FB0085D1AD AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (181, N'Sarah Bloss', N'Texas', N'201403', N'IMPERIAL\Blanca.Carrillo', N'Sarah always shows up to work with a positive attitude. She is friendly to all colleagues and she strives to handle all claims calls with great customer service!', CAST(0x0000A2FB0093ABD3 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (182, N'Trever Lunt', N'Texas', N'201403', N'IMPERIAL\kevin.rekieta', N'I Nominate Trevor because he works hard and tries to help his team whenever he can, Like taking statements for others when they are busy. In that way he is Dependable and makes Imperial more Accessible. He always deals fairly with all of his customers and help to come to a resolution through which ever avenues he can. ', CAST(0x0000A2FB0093ECBD AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (183, N'Jonas Valdez', N'Texas', N'201403', N'IMPERIAL\stephanie.buchanan', N'Jonas is always a hard worker, but during the last month, he went above and beyond for the company.  During the production issues that IT recently struggled with, Jonas worked several 12-18 hour days in a row and also weekends until things were worked out and the company was up and running.  He supports the GARD values in many ways at all times, but during the production issues, he peronally demonstrated these by leading his team, taking responsibility for what needed to be done, and keeping management informed.  He also cares about the IT team as a whole and encourages us all to be better in our jobs and to promote teamwork in the company.', CAST(0x0000A2FB009493DB AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (184, N'Allen Rasbury', N'Louisiana', N'201403', N'IMPERIAL\phyllisc', N'Allen is a litigation adjuster but also handles many of our complex claims.  Many of these claims require working with people who have catastrophic losses.  Allen has contacted medical facilities and worked out the liens on behalf of the unrepresented parties who otherwise would not know where to begin in getting medical bills reduced so they would even get settlement money to use on their own behalf.  Allen does this because of his compassion for those who find themselves "lost" in these difficult circumstances.  His ability to communicate with parties to get the best settlement for everyone concerned is highly effective.    ', CAST(0x0000A2FB00961CEE AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (185, N'Melinda Dehoyos', N'Texas', N'201403', N'IMPERIAL\Allen.Rasbury', N'A third party claimant recently complimented Melinda
Dehoyos on her professionlism demonstrated throughout
the handling of his claim. From what I have observed
Melinda does an outstanding job and represents IFAC
in a fine fashion. Melinda has a positive attitude 
while exhibits fairness in handling first and third
party claims. Melinda has been with IFAC a number
of years and is a valuable team member.   ', CAST(0x0000A2FB0099975F AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (186, N'Allen Rasbury', N'Louisiana', N'201403', N'IMPERIAL\michelles', N'Allen comes in early and works through lunch daily.  He is on the phone all day working to bring resolution to the files assigned to him in a fair and timely manner.  He is never too busy to stop and help someone and is a wealth of knowledge.  Allen is truly an asset to the company.', CAST(0x0000A2FB009A6B7B AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (187, N'ELDRY MEJIA', N'Florida', N'201403', N'IMPERIAL\gladys.ruiz', N'MY CO-WORKER IS ALWAYS WITH A GOOD ATTITUTE AND WHEN HELP IS REQUIRED TO FIND A FILE HE WILL GO OUT OF HIS WAY TO TRY TO LOCATE IT AND THAT IS WHAT A TEAM-WORK IS ALL ABOUT IT !!', CAST(0x0000A2FB00AE97E6 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (188, N'Trevor Lunt', N'Texas', N'201403', N'IMPERIAL\bernie.corral', N'Trevor is everything you want in a rep. He is professional, efficient and has great customer service skills.  He is the ultimate team player and demonstrates GARD values on a daily basis by not only contributing individually but by always making a conceded effort to help out his fellow team mates by taking on additional statements, issuing payments, peer mentoring and even volunteering to work other''s files while they are out of the office. 
 
The next evolution of employee that we have in Level 1 claims begins with people like Trevor who is not only good at his job but has the heart and understanding to go above and beyond for others.


', CAST(0x0000A2FB00AF007D AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (189, N'Josh Holcomb', N'Texas', N'201403', N'IMPERIAL\jbaker', N'Josh always has a postive attitude and a smile on his face no matter the situation. He is always helpful and willing to help no matter what. He was just recently brought on full time, prior he was a contractor with us which proves he''s a great employee if the dept was willing to bring him on full-time. ', CAST(0x0000A2FB00C1CBEA AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (190, N'Jose Hernandez', N'Florida', N'201403', N'IMPERIAL\sandra.delgado', N'He is a very responsible, and reliable employee.  Willing to cooperate and help his fellow co-workers stopping what he is doing to assist in any way possible. He provides excellent customer service in conducting himself in a very courteous and professional way to the extent of even handling calls for his coworkers because of his good business relationship with the client.

He goes beyond the expectations of his work performance each month in completing all his work in a timely manner.', CAST(0x0000A2FB00C8FB10 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (191, N'Sandra Delgado', N'Florida', N'201403', N'IMPERIAL\jose.hernandez', N'Sandra Delgado knows her stuff and she''s more than willing to teach and share her knowledge to others. I''ve personally have learned a great deal with her. She is a fair supervisor that does not let her personal opinion affect her relationship with other co-workers. She gets things done in a timely matter and always finds a solution that benefits all. She will take on roles that are not her own to help whomever needs her. She is definitely, without a doubt, a rock for this place. We are all very fortunate to work with her.', CAST(0x0000A2FB00D466C3 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (192, N'Melissa Dupre', N'Louisiana', N'201403', N'IMPERIAL\Tammy.Ducote', N'I would like to nominate Melissa Dupre.  She is always willing to lend a helping hand on any policy information.  She stops what she is doing when I have a question on a 
NAIC policy.  If I need a question answered on a policy, she takes the time to call the agent or NAIC for me.  Always, always pleasant and always has a smile on her face. I definitely recommend Melissa as employee of the month for March, 2014.', CAST(0x0000A2FB00D7CF45 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (193, N'Trevor Lunt', N'Texas', N'201403', N'IMPERIAL\mdehoyos', N'Mr. Lunt applies his Gard value as  a great team member. He is alway ready and willing to extend his help by taking a warm transfer, a statement, or status call to resolve any issue presented. 
Mr. Lunt demonstrates " Over and Beyond" by following up with his team members who''s warm transfer he''s taken and offers any additional help while he keeps his pending low and worked.  
Mr. Lunt provides Excellant Customer Service by making sure he obtains all information from the caller that he possibly might need in future to resolve and bring a claim to a closure rapidly. Mr. Lunt make sure the callers questions are alway answered before bring the call to an end. 

I have been impressed with Mr. Lunts tatics of handling claims and I am a seasoned adjuster.  My hat''s off Mr. Lunt!!! ', CAST(0x0000A2FB0105440B AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (194, N'Melissa Oehler', N'Texas', N'201403', N'IMPERIAL\sarah.bloss', N'Melissa is incredibly efficient and she constantly impresses with keeping her inventory in check while at the same time treating her customers with courtesy and respect. And if that''s not enough, she has been a huge help and a wonderful teammate helping out with claims that aren''t even hers. ', CAST(0x0000A2FC00DE11E8 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (195, N'JOSE HERNANDEZ', N'Florida', N'201403', N'IMPERIAL\maria.perez', N'JOSE IS A VERY RESPONSIBLE EMPLOYEE.  I HAVE VOTED FOR HIM BEFORE.  HE IS DEDICATED TO HIS WORK, AND HAS A POSITIVE ATTITUDE.  WHEN PROMPTED WITH A QUESTION HE RESPONDS IN THE PROPER MANNER.  HE HAS GOOD CUSTOMER SERVICE SKILLS.   ', CAST(0x0000A2FC0109B22C AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (196, N'Jonas Valdez', N'Texas', N'201404', N'IMPERIAL\jbaker', N'Jonas has been a hard worker since day one of starting here at Imperial, always having a smile on his face and a great attitude no matter what. Even during times where it called for extra hours of work, he went above and beyond for the company to get us back in working order.  He is always willing to do whatever it takes and shows this to his employees as well as the rest of us out on the floor that is he is willing to resolve an issue any way needed even if that means putting in extra hours outside the normal 8-5.  He is an avid supporter of the GARD values and he especially showed this during the System issues we had in February.', CAST(0x0000A308009C79AF AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (197, N'Jose Hernandez', N'Florida', N'201404', N'IMPERIAL\lisa.lowe', N'He is dedicated and always has a positive attitude.  He comes in early and stays late.  He comes in early and stays late.  He is kind and considerate.  He works hard and always double checks his work.  I think he would be an outstanding employee of the month.', CAST(0x0000A31400D50437 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (198, N'Martha Sanchez', N'Florida', N'201404', N'IMPERIAL\alicia.vidal', N'I would like to nominate Martha Sanchez, BI Adjuster for our April Employee of The Month.  
Martha Sanchez is an amazing BI Adjuster. Martha is one of our young senior citizens here but sharp as a whistle.  Martha is extremely knowledgeable and experienced Adjuster with 30 years of great service.  Martha always says yes when asked for assistance. She goes out of her way to help others and is readily available to answer questions or situations requiring her expertise.  Although not assigned claims, she multi-tasks as an adjuster to assist other BI adjusters. Martha assists when other adjusters are on PTO for one or multiple days.  She goes above and beyond her call of duty.  Martha is seen as a mentor to other adjusters.  Often, she is consulted with as her business knowledge and contacts are very helpful in negotiating claims as many attorneys have long standing relationships with Martha.  Her customer service skills both within and outside the office are excellent.  You will never find Martha in a bad mood.  Martha possesses a great personality and a wonderful sense of humor.  Martha is a true example of the GARD Values and should be recognized for her work.  
', CAST(0x0000A31400D72CF3 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (199, N'Stephanie Buchanan', N'Texas', N'201404', N'IMPERIAL\Jamye.Westbrook', N'Stephanie wears many hats in the IT department and she works very well with everyone in the department she has to interact with. She comes up with new ways to organize and mainstream inventory. She is always taking charge of the situation and getting the task completed in a timely manner. She is very helpful in all aspects, and if she doesn''t have the answer she will get it and get back to you. ', CAST(0x0000A31400D83157 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (200, N'Esther Marcos ', N'Florida', N'201404', N'IMPERIAL\maria.brito', N'Esther is a customer service representative. She is very courteous when answering the phone and obtains all the info needed to help the adjusters handle the claims. She is always very helpfull when you need something from her. ', CAST(0x0000A31400D92F4F AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (201, N'Martha Sanchez', N'Florida', N'201404', N'IMPERIAL\teresa.barro', N'I nominated Martha because she''s example as good employee
she''s very friendly, very responsible with all her co-workers, I been nominated before, I considered that she''s the best person that this company can nominated for the employees of the month, she created a good environment. 
every time that you needed she''s there for you.   

She work in the BI department , but any department needed she''s there for you.', CAST(0x0000A31400D93C9E AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (202, N'JOSE HERNANDEZ', N'Florida', N'201404', N'IMPERIAL\sandra.delgado', N'Jose is a very responsible and reliable employee. He has a good and positive attitude with all assignments given, very resourceful.  He is always very encouraged to contact providers/attorneys in an effort to settle and pay what is reasonable for that particular claim even in the diagnostics and other small bills, nothing is ever small enough to not care. He will go out of his way to use his knowledge and abilities to help other co workers in anyway needed, including settlements on their claims. He is a blessing to the PIP department and an honor to work with.', CAST(0x0000A31400E2A80F AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (203, N'Billy Durel', N'Louisiana', N'201404', N'IMPERIAL\michelle.bourque', N'Billy is always helpful to his customers; he is always in early and gets his work done. He is also always there for his co-workers when we have a question or when you need something. ', CAST(0x0000A31400E8B3E7 AS DateTime))
GO
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (204, N'Rosana Mecias ', N'Florida', N'201404', N'IMPERIAL\luisa.perez', N'Rosana is always willing to provide a good service. She is ready to help in any task. 
Without hesitation she answers any questions you have or finds anything for you. 
She is really responsible I have never heard any critics or complains about her. ', CAST(0x0000A315007E4A57 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (205, N'Sandra Delgado ', N'Florida', N'201404', N'IMPERIAL\marlene.fernandez', N'I would like to nominate Sandra Delgado for employee of the month. I have worked with her for several years, and she has always been a great all around employee,not only  through her work but also with other employees , Sandra is a very caring person, she has a very positive attitude and always goes Over and Beyond her call of duties not only with each and every one of her files but also in helping others in her department, she truely is an asset to Imperial in many ways and very deserving of this recognition .          ', CAST(0x0000A315008A8139 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (206, N'Sandra Delgado', N'Florida', N'201404', N'IMPERIAL\jose.hernandez', N'Sandra is fair. She can perfectly distinguish her personal preferences and her professional duties as a supervisor. Her doors are always opened if we are in need of assistance - she will always find a solution to whatever question, or guide us to the right direction to find the solution. She always has a smile painted on her face and a great sense of humor follows it - but she also has a great sense of responsibility and pride on her work which inspires those around her. She is definitely a key employee to this company. ', CAST(0x0000A316006E1500 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (207, N'Billy Durel', N'Louisiana', N'201404', N'IMPERIAL\darla.pitre', N'This month we have had a number of storms come through resulting in a larger number of claims.  Billy always treats our customers with respect, kindness and understanding during their times of need.  I think that is a great quality.', CAST(0x0000A31700AE947E AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (208, N'MERCEDES PILOTO', N'Florida', N'201404', N'IMPERIAL\marta.villasuso', N'SHE IS AN EXTREMELY HELPFULL PERSON ALWAYS GOING OUT OF HER WAY TO HELP EVERYONE IN ALL THE DEPARTMENTS
SHE IS CUSTOMER SERVICE PERSONIFIED WITH CUSTOMERS, AGENTS AND EMPLOYEES, SHE GOES OUT OF HER WAY TO DO WHAT THEY ASK OF HER AND IN CASE SHE CAN''T HELP SHE FINDS WHO CAN 
EVERYONE KNOWS HER AND CAN COUNT ON HER 110%', CAST(0x0000A31700AEA848 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (209, N'JOANA BARRERA', N'Florida', N'201404', N'IMPERIAL\carmen.osorio', N'THIS IS A YOUNG LADY THAT GOES OUT OF HER WAY IN EVERY FILE THAT SHE HANDLES IN THIS TUFF PIP WORLD. IT''S A PLEASURE WORKING SIDE BY SIDE WITH THIS PIP ADJUSTER. I HAVE BEEN IN THE PIP FIELD FOR MANY YEARS AND SEEN IT ALL!! VERY FEW PEOPLE IMPRESS ME LIKE THIS ADJUSTER DOES. I THINK SHE DESERVES TO HAVE THAT LION ON HER DESK...
OSORIO', CAST(0x0000A31700AF4CCB AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (210, N'Jose Hernandez', N'Florida', N'201404', N'IMPERIAL\alex.gonzalvez', N'Jose is probably one of the most helpful adjusters here. Comes in early and leaves late.
Very efficient with his work
Always willing to help others ', CAST(0x0000A31700B08686 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (211, N'Stephanie Buchanan', N'Texas', N'201404', N'IMPERIAL\stacey.morgan', N'Stephanie has assisted me several times this month when we were short handed.  She is always eager to learn something new, and she is always available to help no matter what she has on her plate.', CAST(0x0000A31700B1D65F AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (212, N'STACEY MORGAN', N'Texas', N'201404', N'IMPERIAL\lrodriguez', N'Stacey Morgan has gone over and beyond for Imperial this past month.  She has been working really hard with our New Web-site and has been training employees for this new exciting site. There are many other projects she has been working on for Imperial. Stacey is very polite and always greets you with a smile.  ', CAST(0x0000A31700B90F12 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (213, N'Michelle Bourque', N'Louisiana', N'201404', N'IMPERIAL\Mike.Reyna', N'Michelle, has been very good about taking care of all clerical work for us even during this time of heavy volume, making sure all assignments are made and doing ISO and ACk letters for us. She has even caught a claim with no coverage before it was assigned and saved expenses associated with that. She is also very positive and a delight to deal with at all times!', CAST(0x0000A31700BC2121 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (214, N'Belkis Tejeda', N'Florida', N'201404', N'IMPERIAL\fernando.rodriguez', N'Belkis is always willing to help anyone no matter the task.Belkis works very hard and smart in whatever task she is assigend to.She brings a postivie vibe to the work force and she is always there when you need her.  ', CAST(0x0000A31700D7BEA0 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (215, N'RONIEL BIAS', N'Louisiana', N'201404', N'IMPERIAL\fayeg', N'SHE HAS TAKEN POSITION OF RECEPTIONIST, AND HELPS THE CALLS
GET FROM A TO B, SHE WILL TAKE INFORMATION AND HAVE PEOPLE RETURN CALLS IF NECESSARY, WHEN THEY GET FRUSTRATED TRYING TO GET THROUGH TO THE PEOPLE THEY ARE TRYING TO REACH.
SHE HAS OFFERED HER HELP IN OTHER AREAS AS WELL. ', CAST(0x0000A31701057BC4 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (216, N'Tracie Lang', N'Louisiana', N'201404', N'IMPERIAL\michelles', N'Tracie always keeps a happy disposition on the phone even when dealing with a difficult customer.  When volunteers are needed you can count on Tracie to help.  ', CAST(0x0000A317010E1730 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (217, N'Melissa Dupre', N'Louisiana', N'201404', N'IMPERIAL\Tammy.Ducote', N'There are so many people that are worthy of being nominated, but again will nominate Melissa, Underwriting Manager.  She is always cheerful, willing to help me if any questions regarding NAIC; has been with the company for many, many years so she is very dedicated employee.  I have never seen a frown on her face.', CAST(0x0000A3180093ADD2 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (218, N'Joanna Harris', N'Texas', N'201404', N'IMPERIAL\Ray.Brown', N'Early Settlement Offers ( ESO''s) are a critical metric in controlling our Bodily Injury indemnity results, in addition to providing often exceptional customer service.  During the month of March, JoAnna had an exceptional accomplishment, 24 ESO''s for the month. This monthly result represents a high water mark for an Bodily Injury Level Two adjuster. The twenty-four settlements  represented 38% of the ESO''s settled in March. Level Two had a  grand total of 63 for the month. Additionally JoAnna also received a favorable Arbitration award in April! The adverse insurance carrier will be issuing a payment of $6,703.99 to Imperial Fire & Casualty.', CAST(0x0000A31800A66225 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (219, N'Stacey Morgan', N'Texas', N'201404', N'IMPERIAL\pzimmerman', N'She is dependable, respectful, kind and thoughtful. Always has a smile and cheerful greeting to all.

This month she has worked many hours on the new website that will be launched on 04/29/2014.

She gave training classes to the underwriting department that was very informative.', CAST(0x0000A31800EE9646 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (220, N'Stacey Morgan', N'Texas', N'201404', N'IMPERIAL\posborn', N'Stacey is a very friendly co-worker. She is very helpful and informative when we have questions.  She has worked a lot of hours on the new website which is easier to navigate.
She is an asset to this company.', CAST(0x0000A31800EF4F51 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (221, N'Stephanie Buchanan', N'Texas', N'201405', N'IMPERIAL\Jamye.Westbrook', N'Stephanie has repeatedly shown GARD values working with her team mates. She is very helpful and knowledgeable. She always is her early and willing stays late to make sure that everything is taken care of. ', CAST(0x0000A33900CD0F5E AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (222, N'Pedro Caballero', N'Florida', N'201405', N'IMPERIAL\fernando.rodriguez', N'Pedro is an influential person in the Miami office. He is a great example to his peers due to his consistent hardwork and discipline. Pedro is always on time and is always open to help anyone that is in need of assistance. Pedro goes beyond the call by exceeding expectations in work and always brings positivity to the office.  ', CAST(0x0000A33900CF2105 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (223, N'Belkis Tejeda ', N'Florida', N'201405', N'IMPERIAL\marlene.fernandez', N'Belkis is a great example of the imperial Gard value, she is dedicated and hard worker with a great friendly personality, she investigates and works her files very carefully and very thorough , making sure all claims are closed correctly with the insured as well as keeping the Companies best interest in mind. 
', CAST(0x0000A33900CF5ACA AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (224, N'Billy Durel', N'Louisiana', N'201405', N'IMPERIAL\michelle.bourque', N'Billy is always there for his customers. He helps you when needed, and he is very good at what he does. He comes in early; stays late--he is a very dedicated worker. He deserves to be acknowledged ', CAST(0x0000A33900D1E9A7 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (225, N'Sue Willis', N'Louisiana', N'201405', N'IMPERIAL\michelles', N'Sue is an asset all the way around.  She works very hard and is reliable.   Sue wears many hats up here! Outside of being a litigation adjuster she assists Phyllis on some special projects, handles some our computer issues, always has time to answer a fellow co-worker''s questions and always steps up to the plate when we have catastrophe situations.   Sue is a great resource to our office and the company.', CAST(0x0000A33900D44E11 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (226, N'Joseph Daugereaux', N'Louisiana', N'201405', N'IMPERIAL\terir', N'During the new website enrollment, Joseph did an outstanding job with our agents. He had to get my assistance many times when he didn''t understand the problem the agent was having BUT asked to stay on the line with us so he would be able to handle it the next time it came up. He was upfront with the agent that he needed to get someone else to assist so they were not frustrated when he enlisted my help. This enrollment process is new procedure and Joseph embraced learning the ''tricks'' to assist the agent to get them on their way to quoting and selling IMPERIAL!', CAST(0x0000A33900E82950 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (227, N'Tammy Ducote', N'Louisiana', N'201405', N'IMPERIAL\boydg', N'Tammy is very personable and courteous to everyone she speaks with. She has a desire to help you whatever your needs are. she is always available to take care of customers and spends extra hours at the office to take care of matters. She goes the extra mile to help customers and takes ownership of the situation. ', CAST(0x0000A33900F2DA1A AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (228, N'Roberto Jorge', N'Florida', N'201405', N'IMPERIAL\guillermo.callejas', N'Roberto is an exemplary employee, and is a true example of what our GARD values are. He is dependable, reliable, and a value and asset to our company.
 There have been may times where we have needed a last minute EUO taken and he has always answered the call and is always willing to help. He is kind hearted, easy to get along with and brings a great attitude to the working environment. He always puts the best interest of the office first and is considered a true professional by his peers! ', CAST(0x0000A33A01025989 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (229, N'CHRISTINE HAWKINS', N'Louisiana', N'201405', N'IMPERIAL\fayeg', N'CHRISTINE IS ALWAYS EAGER TO HELP WHENEVER SHE IS CALLED ON.
WHETHER ITS A CLAIM OR ANYTHING OTHER, SHE IS ALWAYS EAGER TO HELP, SHE HAS BEEN CALLED UP FRONT ON MANY OCCASIONS,
AND HAS ALWAYS CAME FORWARD AND TRIED TO HELP OUT.', CAST(0x0000A33B008E917B AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (230, N'Ryan Selby-Karney', N'Texas', N'201405', N'IMPERIAL\Blanca.Carrillo', N'Ryan has a great attitude and is a very wonderful person to work alongside. He is always willing to help and I can always hear his great customer service on every call!', CAST(0x0000A33B0097DA8D AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (231, N'Tammy Ducote', N'Louisiana', N'201405', N'IMPERIAL\christine.hawkins', N'Ms. Tammy is a very caring person, she is very hard working, does not hesitate to stay after hour or come during the weekend. She always treat the people with patience and respect. She always helps fellow co-workers and does it gladly. I think that her hard work and her manner with other co-workers are equal to an employee of the month for IFAC', CAST(0x0000A33B009A0293 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (232, N'Sue Willis', N'Louisiana', N'201405', N'IMPERIAL\traciel', N'Sue has worked in the claims field many years and displays a vast knowledge and exceptional skills when it comes to her job as a litigation adjuster for IFAC.  She shows courtesy to others when dealing with a claim and is also courteous to her co-workers. Sue is the one that will always make effort to contact IT if there is a problem within our office regarding the computers or phones. She is a dedicated employee and is on time, each and everyday.', CAST(0x0000A33B009A6229 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (233, N'Marta Sanchez', N'Florida', N'201405', N'IMPERIAL\rosana.mecias', N'Martha Sanchez is very gentle and excellent co-worker. She is always willing to help others and is a wonderful person. MARTHA is a hardworking person and a team player. ', CAST(0x0000A33B009A8AE9 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (234, N'Jaime Wallace', N'Texas', N'201405', N'IMPERIAL\lizw', N'Jaime is resourceful, polite and always willing to help. As busy as she is managing the ICEBox team and working with the DEV team, she always responds in a timely manner and goes out of her way to help find a solution to a problem. She always sounds like she has a smile on her face which makes her a  delight to work with.', CAST(0x0000A33B009C5D31 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (235, N'Martha Sanchez', N'Florida', N'201405', N'IMPERIAL\alicia.vidal', N'I would like to nominate Martha Sanchez, BI Adjuster for our May Employee of The Month.  
Martha Sanchez is an amazing BI Adjuster. Martha is one of our young senior citizens here. Martha is extremely knowledgeable and experienced Adjuster with 30 years of great service.  Martha always says yes when asked for assistance. She goes out of her way to help others and is readily available to answer questions or situations requiring her expertise.  Although not assigned claims, she multi-tasks as an adjuster to assist other BI adjusters. Martha assists when other adjusters are on PTO for one or multiple days.  She goes above and beyond her call of duty.  Martha is seen as a mentor to other adjusters.  Often, she is consulted with as her business knowledge and contacts are very helpful in negotiating claims as many attorneys have long standing relationships with Martha.  Her customer service skills both within and outside the office are excellent.  You will never find Martha in a bad mood.  Martha possesses a great personality and a wonderful sense of humor.  Martha is a true example of the GARD Values and should be recognized for her work.  
', CAST(0x0000A33B00A33330 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (236, N'Cary Machado', N'Florida', N'201405', N'IMPERIAL\maritza.morera', N'She exceeds the expectations of her job,consistently demonstrating a positive attitude, working toward the team goals and putting her own interests aside. She shows perseverance and commitment to do her best in the face of overwhelming obstacles. The best worker ever!!!', CAST(0x0000A33B00B03F28 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (237, N'Jonas Valdez', N'Texas', N'201405', N'IMPERIAL\jbaker', N'Jonas is always helpful and goes beyond his job duties to get things done. He lives the Imperial GARD values every day. 

This month when our servers went down crashing all of our systems he went to the data center and stayed there until everything was fixed which I understand was well past closing time. 

I think he deserves employee of the month for May.', CAST(0x0000A33B00C0F8B4 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (238, N'CARMEN OSORIO', N'Florida', N'201405', N'IMPERIAL\maria.perez', N'SHE IS A DEDICATED EMPLOYEE.  SHE IS CONCENTRATED IN HER WORK AND GIVES IT ALL THE ATTENTION.  SHE HAS A POSITIVE ATTITUDE. WHEN ASKED A QUESTION SHE RESPONDS IN A VERY POLITE MANNER.   ', CAST(0x0000A33B0101E68A AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (239, N'Ethan Porter', N'Texas', N'201405', N'IMPERIAL\derek.dodson', N'Ethan is the very epitome of hard-working, conscientious and dependable.  Without ever being asked, he is here after hours whenever neccessary and even on the weekends.  He is the dependable backbone that every growing company needs because he WILL get it done because he won''t let it NOT get done.  He is not only doing this for his desk or even for his team, staying late into several evenings earlier this month to assist another representative on a different team who had gotten a bit behind and needed a hand up.  As a young claims adjuster, his growth in his file quality and efficiency in disposing of files in the correct way are impressive, but it is nothing compared to his simple willingness to get the job done no matter what it takes.  Ethan is what Imperial is all about and I''m proud to be associated with a man of his caliber.', CAST(0x0000A33B010BC1E2 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (240, N'Maria Eusse', N'Florida', N'201406', N'IMPERIAL\yan.sanchez', N'I think Maria is an example to which everyone should follow she is always very professional, courteous and knows what to do. She focuses on the work in front of her and knows how to handle anything thrown at her. She should be employee of the month because she embodies what an adjuster should do and behave. ', CAST(0x0000A35400C023B1 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (241, N'Billy Durel', N'Louisiana', N'201406', N'IMPERIAL\darla.pitre', N'Billy is always working hard to satisfy customers and handles them in a very professional manner,  He is always helping me by alerting me if a risk no longer meets the guidelines and that helps save the company on homes that may turn in more losses.', CAST(0x0000A35400C09324 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (242, N'Billy Durel', N'Louisiana', N'201406', N'IMPERIAL\michelle.bourque', N'Billy helps his customers and his fellow co-workers; when needed. He goes above and beyond his duty to help them. He comes early to work; and if needed; he will stay late. He deserves to be the employee of the month. He makes everyone smile with his personality. And at the end of a hard work day or week; it is awesome to smile', CAST(0x0000A35400CF4125 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (243, N'Teresa Barro ', N'Florida', N'201406', N'IMPERIAL\fernando.rodriguez', N'I am nominating Teresa Barro from the property damage department because she shows excellent customer service. Teresa has a natural gift to deal with the customer and always treats them with respect. There are many times that disgruntled customers call her but she always keeps her composer and professionalism when helping them with their situation. Teresa has such a great way in explaining things that many times the disgruntled customer apologizes for their actions. Teresa’s patience and ongoing customer satisfaction is why I am nominating her for employee of the month. Teresa also brings a positive vibration into our work environment. Teresa always has a smile in her face and is always there to cheer her co workers up.   ', CAST(0x0000A35400CFACD5 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (244, N'Trevor Lunt', N'Texas', N'201406', N'IMPERIAL\Dawn.Bridges', N'I work with MANY great, helpful people here at Imperial but Trevor is pro-active, helpful and is always willing to drop whatever he is doing to help someone in need. He helps those who have been with the company for a while and those who are new. Not to mention he is great at handling CAT claims!!!!', CAST(0x0000A35400E238E7 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (245, N'Juan C Delgado ', N'Florida', N'201406', N'IMPERIAL\maria.brito', N'JUAN IS VERY ACCESSIBLE. HE IS ALWAYS IN A GOOD MOOD AND VERY HELPFUL. HE HAS A GREAT ATTITUDE AND IS VERY RESPECTFULL. HE IS ALSO VERY GOOD WHEN DEALING WITH THE CLIENTS. ', CAST(0x0000A35400E313C8 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (246, N'Ryan Selby', N'Texas', N'201406', N'IMPERIAL\lrodriguez', N'Ryan is an out standing homeowners underwriter.  In the past few months Ryan is worked extra hard by coming in early every day to underwrite new business applications.  He has really gone over and beyond for Imperial.  Ryan is friendly and has great customer service.  Ryan will always greet you with a smile.  ', CAST(0x0000A355007E22DB AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (247, N'Arjan Lakhani', N'Florida', N'201406', N'IMPERIAL\juana.najera', N'As a PIP adjuster, He takes initiative and accepts and carries out additional responsibilities beyond regular job assignments. He works independent but he also knows how to do team work.  Any information that we need from him, he does it promptly. He loves to support other colleagues.  He goes beyond the call of duty, polite and friendly, good customer service, loyal to the company and carries a positive attitude towards work.', CAST(0x0000A355008B377C AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (248, N'Ryan Selby-Karney', N'Texas', N'201406', N'IMPERIAL\pzimmerman', N'He is dependable, dedicated and friendly. He has a wonderful working relationship with his co-workers and agents. 

This past month he has come in early and has made sure all underwriting on homeowner applications and renewals are current. He works extremely hard to keep our loss ratio down.
His customer service skills are great. I''ve had many positive comments from agents and insured''s.', CAST(0x0000A355009F4515 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (249, N'Ryan Selby-Karney', N'Texas', N'201406', N'IMPERIAL\Andrea.Ward', N'Ryan exemplifies what it means to be a great employee.  He provides world class service to our agents and policy holders, supports his fellow team members, and always has a smile on his face.  Ryan comes in early every morning to get a jump on daily processing to ensure we exceed our daily service goals.  He is courteous and friendly, hard working, and does everything that is asked of him (sometimes before he is even asked!)', CAST(0x0000A35500A2FA8B AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (250, N'JOSEPH  DAUGEREAUX', N'Louisiana', N'201406', N'IMPERIAL\fayeg', N'
I HAVE NOMINATED JOSEPH, BECAUSE HE IS ALWAYS WILLING TO HELP. HE HAS HELPED ME TO MANY TIMES TO COUNT. IT DIDN''T MATTER WHAT THE TASK WAS, IF HE WAS BUSY AT THE TIME, HE ALWAYS SHOWED UP, A LITTLE LATER, OR CALLED ME BACK TO SEE IF IT WAS TAKEN CARED OF.', CAST(0x0000A35500CF7A35 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (251, N'Ryan Selby-Karney', N'Texas', N'201406', N'IMPERIAL\posborn', N'Ryan is a dedicated employee.  He is very friendly with the agents; customers and his fellow co-workers. He comes in early and stays late to get the job done.  He is an asset to the Homeowner team and to the company.  ', CAST(0x0000A3560076E105 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (252, N'Ryan Selby', N'Texas', N'201406', N'IMPERIAL\jill.schmalholz', N'Ryan is a conscientious worker and has been coming in early to make sure the underwriting does not fall behind.
When handling calls with both clients and agents, his calls are always upbeat. Any person  who speaks with Ryan can feel that the genuine "you are important to Imperial" in his voice.
Ryan explains issues thoroughly, and resolves the problems with a very pleasant attitude.', CAST(0x0000A356007B9CC8 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (253, N'Jamye Westbrook', N'Texas', N'201406', N'IMPERIAL\shaun.herschbach', N'Jamye has taken charge of the help desk and worked to make things go as smooth as possible during what has been quite a chaotic month.  With the expansion into the 1st floor executive offices, the move of the board room, set up of additional conference rooms and a few short notice moves of the development staff along with all their required hardware (down to the first floor one week, back upstairs the next, along with rewiring for the new layout).   

She''s done a fantastic job and gone above the call of duty in several instances whether coming in early, staying late, or going out (sometimes on extremely short notice) to track down needed supplies.', CAST(0x0000A35600865325 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (254, N'Kevin Burger', N'Texas', N'201406', N'IMPERIAL\stacey.morgan', N'Kevin has been instrumental in helping IT transition into their new work environment this month. He pushes his team to work harder and smarter every day.  He also developed a new tool that will alert IT staff when there are possible system problems before we get calls from agents. I have seen a lot of positive change in the department due to his leadership. ', CAST(0x0000A3570087F4D2 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (255, N'Ves Garrett', N'Louisiana', N'201406', N'IMPERIAL\brendab', N'Ves is very quiet and sits in his office and does his work but if anyone needs anything they can always call and he will always go help them.  Always willing to lend a helping hand', CAST(0x0000A35700B3F846 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (256, N'Juan Enriquez', N'Louisiana', N'201406', N'IMPERIAL\tamarica.trent', N'Juan is a very dictated young man. He is always wiling to help in anyway he can. Juan always greets his co-workers and whom ever he comes in to contact with, with a warm smile.', CAST(0x0000A35700B443C8 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (257, N'Yessenia Mirabal', N'Florida', N'201406', N'IMPERIAL\Carmen.Lambert', N'Yessenia has been successful in uncovering suspect claims and following through on obtaining the testimony and evidence needed for the company to avoid paying fraudulent claims. She goes above and beyond the call of duty by going out to wherever the lead takes her speaking to officers, insureds, claimants, clinics, witnesses, agents , adjusters from other companies, etc. I believe she has made a very positive impact on the companies results. ', CAST(0x0000A35700B4A8C5 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (258, N'Faye Goutro', N'Louisiana', N'201406', N'IMPERIAL\Olivia.Williams', N'Faye is always smiling and always there to help no matter how busy she may be.  She has the best work ethic you could ask for..  I love the personnel that I work with! ', CAST(0x0000A35700BCD6E0 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (259, N'Billy Durel', N'Louisiana', N'201406', N'IMPERIAL\Tammy.Ducote', N'I vote for Big Bill.  He is always on time, always very polite, always there to help someone, is very knowledgeable.  He is a terrific person and always keeps the moral in the whole building up.  We love him to death.', CAST(0x0000A35700BD0BF5 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (260, N'Faye Goutro', N'Louisiana', N'201406', N'IMPERIAL\melissad', N'Faye is the most helpful person I know; the job is never too big or too small.  She always jumps in with both feet and dose the best job she can do.
The smile that is always on her face is a bonus!
', CAST(0x0000A35700BDA2B3 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (261, N'FAYE GOUTRO', N'Louisiana', N'201406', N'IMPERIAL\christine.hawkins', N'Ms. Faye is always ready to help anybody that asks. She is always very cordial and sweet to everybody. I have never heard her complain about any task that has been set before her. She is a great friend and coworker ', CAST(0x0000A35700C44306 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (262, N'Maria Eusse ', N'Florida', N'201406', N'IMPERIAL\marlene.fernandez', N'Maria started as a receptionist and has worked her way to one of our better pd adjusters, she is very professional ,  dedicated and hard worker, she is a great person and daily continues to go over and beyond the call of duty when working her files , she is a great asset to Imperial ', CAST(0x0000A35700CDA855 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (263, N'Darla Pitre', N'Louisiana', N'201406', N'IMPERIAL\Billy.Durel', N'When it comes to customer service Darla should be the example for all of IFAC.  As an underwriter, Darla deals with the policyholders, agents and internal customers.  As a HO adjuster, Darla provides and offers and great deal of help to me when it comes to a variety of issues.
Sitting near Darla I have the opportunity to hear her dealing with the customers.  We all know the old saying that patience is a virtue.  Based on this, Darla is an extremely virtuous person.  Based on the side of the conversation I hear at times, one can tell that she is dealing with an upset person.  The tone, calmness and professionalism that I hear helps to defuse the situation and must be reassuring to the caller on the other end. ', CAST(0x0000A35700CDBA88 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (264, N'Jeniffer Starnes', N'Texas', N'201406', N'IMPERIAL\Allen.Rasbury', N'Jennifer is a trusted, diligent,and responsible employee
who satisfactorily performs her adjuster tasks in a timely
manner. She has my highest recommendation for employee of
the month.', CAST(0x0000A35700D0C573 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (265, N'Cary Machado', N'Florida', N'201406', N'IMPERIAL\leydi.betancourt', N'Cary is a great supervisor, always available to help everyone in her department out, she even helps out the claims department when they come in with questions or concerns. She is here very early every day and sometimes stays very late working hard. ', CAST(0x0000A35700FF64AA AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (266, N'Ryan Hatchell', N'Texas', N'201406', N'IMPERIAL\ethan.porter', N'I cannot think of more deserving person than Ryan as employee of the month. He has many qualities to prove it including the amount of pride he takes in working for Imperial, the “get the job done” no matter what attitude in staying after hours or sacrificing his time to help other co-workers. However, none of those qualities speak as highly to his character as the fact that no matter how bad your day may be going, a simple laugh or joke from him can completely change the entire atmosphere of the office. That my friends is fairy dust…', CAST(0x0000A35701185D4D AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (267, N'Darla Pitre', N'Louisiana', N'201407', N'IMPERIAL\michelle.bourque', N'She is a very sweet, caring person when she is handling phone calls. She listens and helps customers out when ever they have trouble or have questions about anything; if she doesn''t know it; she makes sure that she ends that phone call letting customer know what he or she needs to do. She is very helpful and cheerful co-workers. I have never heard her complain one time----even when her phone rings the minute she hangs up ', CAST(0x0000A36F010489DB AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (268, N'JOSEPH DAUGEREAUX', N'Louisiana', N'201407', N'IMPERIAL\fayeg', N'
I NOMINATE JOSEPH DAUGEREAUX, HE IS AN EXCLELLENT CHOICE.
HE IS ALWAYS READY TO HELP, AND IF HE CAN''T HELP RIGHT AWAY.
HE WILL CALL YOU BACK OR HE WILL COME TO YOUR DESK TO FIND OUT THE ISSUE''S AT HAND. I VOTE FOR JOPSEPH. HE HAS HELPED
OUT MANY TIMES.', CAST(0x0000A36F0108BAE0 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (269, N'Ryan Hatchell', N'Texas', N'201407', N'IMPERIAL\Barbara.Grimes', N'He stays as long as he needs to to get his work done. He does his best to provide good service to his clients, and he works hard to make sure he does the best that he can.', CAST(0x0000A373009C592D AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (270, N'Maribel Humaran', N'Florida', N'201407', N'IMPERIAL\rosana.mecias', N'Maribel is such a special person, she is always willing to help others. She is a very hardworking and organized , Maribel is one of the best co-workers that I''ve ever had.  ', CAST(0x0000A373009C88AA AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (271, N'Faye Gautro', N'Louisiana', N'201407', N'IMPERIAL\darla.pitre', N'Faye is always willing to help anyone who needs help and is always friendly to anyone she comes in contact with, always has a smile on her face. If you need help catching up she is there for you.', CAST(0x0000A373009CDBC4 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (272, N'Faye Gautrau', N'Louisiana', N'201407', N'IMPERIAL\Tammy.Ducote', N'Faye is always there to lend a helping hand, anything you ask of her.  She goes out of her way to come get mail when we''re in a last minute rush and she is just a great person.
Very polite, kind and just an overall great person.', CAST(0x0000A373009E9FB3 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (273, N'Teresa Barro ', N'Florida', N'201407', N'IMPERIAL\marlene.fernandez', N'Teresa is one of our best adjusters, she has a great attitude and is always very helpful not only with her coworkers but with our insured, her claim files are always worked in a professional manner with both our insureds and claiments ,she has always demonstrated to be a dedicated employee going over and beyond the call of duty   ', CAST(0x0000A37300A20B89 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (274, N'FAYE GOUTRO', N'Louisiana', N'201407', N'IMPERIAL\christine.hawkins', N'Ms. Faye is very hard working and she is always willing to help. She always has a smile on her face and is not scared to learn new things. A lot of times she will go out of her way to help when she can. ', CAST(0x0000A37300A23C93 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (275, N'Ya Sanchez', N'Florida', N'201407', N'IMPERIAL\oscar.rueda', N'Yan is an excellent coworker, very helpful, yan is responsible to coordinate the appraisal assigments, also he is the front line coordinate supplement, he is very efficient, when you need help he is always there for the lion roar call ', CAST(0x0000A37300A3CBE6 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (276, N'Yvonne Castillo', N'Florida', N'201407', N'IMPERIAL\maribel.humaran', N'Yvonne, is a great employee. 
She is always going out of her way to assist all her fellow co-workers. Yvonne, is a valuable member to our company always following our GARD values. Her humor puts a smile on everyone face while her job is always number 1.  ', CAST(0x0000A37300A5CD6A AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (277, N'Michael Wodek', N'Texas', N'201407', N'IMPERIAL\Cassandra.Cline-Gera', N'Mike is always there to lend a helping hand whenever he is needed.  He makes the working environment feel very friendly and happy.  When you ask something of him, he makes sure to get it done.', CAST(0x0000A37300ECAA16 AS DateTime))
INSERT [dbo].[EoMNominations] ([ID], [Name], [Location], [Month], [Nominator], [Reason], [SystemTS]) VALUES (278, N'Jonas Valdez', N'Texas', N'201407', N'IMPERIAL\shaun.herschbach', N'While I can honestly say the entire infrastructure team is deserving of employee of the month having put in not only their regular work weeks, but given up every weekend this past month to get our entire business environment moved to a new cage in our data center, Jonas as the ring leader has been on top of it all.  He''s worked extra long hours each work day and then been in the trenches with his team at the data center leading them through the long weekend hours.  All the while he''s kept the communication flowing extremely well.  He also worked through the weekend of the 19th with Billy Brown in New Orleans getting the new NAIC office up running and connected to Imperial.  And all of this has been done without a blip in our systems (thanks in part to the extra hours put in by the dev staff to test the systems as they were brought up by the infrastructure team).', CAST(0x0000A376008B035C AS DateTime))
SET IDENTITY_INSERT [dbo].[EoMNominations] OFF
